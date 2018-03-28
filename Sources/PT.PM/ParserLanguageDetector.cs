using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace PT.PM
{
    public class ParserLanguageDetector : LanguageDetector
    {
        private readonly static Regex openTagRegex = new Regex("<\\w+>", RegexOptions.Compiled);
        private readonly static Regex closeTagRegex = new Regex("<\\/\\w+>", RegexOptions.Compiled);

        private Language previousLanguage;

        public TimeSpan LanguageParseTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan CheckParseResultTimeSpan { get; set; } = TimeSpan.FromMilliseconds(100);

        public override DetectionResult Detect(string sourceCode, IEnumerable<Language> languages = null)
        {
            List<Language> langs = (languages ?? LanguageUtils.Languages.Values).ToList();
            langs.Remove(Uncertain.Language);
            // Any PHP file contains start tag.
            if (!sourceCode.Contains("<?"))
            {
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Php"));
            }
            // Aspx and html code contains at least one tag.
            if (!openTagRegex.IsMatch(sourceCode) || !closeTagRegex.IsMatch(sourceCode))
            {
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Aspx"));
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Html"));
            }
            var sourceCodeFile = new CodeFile(sourceCode);
            var parseUnits = new Queue<ParserUnit>(langs.Count);

            langs = langs
                .GroupBy(l => l.CreateParser())
                .Select(l => l.First())
                .ToList();

            if (langs.Count == 1)
            {
                return new DetectionResult(langs[0]);
            }

            foreach (Language language in langs)
            {
                Thread thread = new Thread((object obj) =>
                {
                    ((ParserUnit)obj).Parse(sourceCodeFile);
                });
                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;

                ParserUnit parseUnit = new ParserUnit(language, thread);
                thread.Start(parseUnit);

                parseUnits.Enqueue(parseUnit);
            }

            int checkParseResultMs = (int)CheckParseResultTimeSpan.TotalMilliseconds;
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Check every parseUnit completion every checkParseResultMs ms.
            while (parseUnits.Any(parseUnit => parseUnit.IsAlive) &&
                   stopwatch.Elapsed < LanguageParseTimeout)
            {
                ParserUnit parseUnit = parseUnits.Dequeue();
                parseUnits.Enqueue(parseUnit);

                if (parseUnit.IsAlive)
                {
                    Thread.Sleep(checkParseResultMs);
                }
                else
                {
                    if (parseUnit.ParseErrorCount == 0 && parseUnit.Language.Key != "Aspx")
                    {
                        break;
                    }
                }
            }

            int resultLastErrorOffset = 0;
            ParserUnit result = null;
            int resultErrorsCount = int.MaxValue;

            foreach (ParserUnit parseUnit in parseUnits)
            {
                parseUnit.Abort();

                List<ParsingException> parseErrors = parseUnit.Errors.Where(error =>
                     error is ParsingException parsingException && !(parsingException.InnerException is ThreadAbortException))
                    .Cast<ParsingException>()
                    .ToList();

                int currentLastErrorOffset = parseErrors.LastOrDefault()?.TextSpan.End ?? int.MaxValue;
                if (currentLastErrorOffset > resultLastErrorOffset)
                {
                    resultLastErrorOffset = currentLastErrorOffset;
                    result = parseUnit;
                }
                else if (currentLastErrorOffset == resultLastErrorOffset)
                {
                    int errorCount = parseErrors.Count;
                    if (errorCount < resultErrorsCount)
                    {
                        resultErrorsCount = errorCount;
                        result = parseUnit;
                    }
                    else if (errorCount == resultErrorsCount && previousLanguage != null)
                    {
                        result = new ParserUnit(previousLanguage, null);
                    }
                }
            }

            if (result != null)
            {
                previousLanguage = result.Language;

                return new DetectionResult(result.Language, result.ParseTree,
                    result.Errors, result.Infos, result.Debugs);
            }

            return null;
        }
    }
}
