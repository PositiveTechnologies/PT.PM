using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common.Files;
using PT.PM.LanguageDetectors;

namespace PT.PM
{
    public class ParserLanguageDetector : LanguageDetector
    {
        private static readonly Regex openTagRegex = new Regex("<\\w+>", RegexOptions.Compiled);
        private static readonly Regex closeTagRegex = new Regex("<\\/\\w+>", RegexOptions.Compiled);

        private Language previousLanguage;

        public TimeSpan LanguageParseTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan CheckParseResultTimeSpan { get; set; } = TimeSpan.FromMilliseconds(100);

        public override DetectionResult Detect(TextFile sourceFile, IEnumerable<Language> languages = null)
        {
            List<Language> langs = (languages ?? LanguageUtils.LanguagesWithParser).ToList();

            // Any PHP file contains start tag.
            if (!sourceFile.Data.Contains("<?"))
            {
                langs.Remove(langs.FirstOrDefault(l => l == Language.Php));
            }
            // Aspx and html code contains at least one tag.
            if (!openTagRegex.IsMatch(sourceFile.Data) || !closeTagRegex.IsMatch(sourceFile.Data))
            {
                langs.Remove(langs.FirstOrDefault(l => l == Language.Aspx));
                langs.Remove(langs.FirstOrDefault(l => l == Language.Html));
            }
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
                Thread thread = new Thread(obj =>
                {
                    ((ParserUnit)obj).Parse(sourceFile);
                },
                MaxStackSize);
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
                    if (parseUnit.ParseErrorCount == 0 && parseUnit.Language != Language.Aspx)
                    {
                        break;
                    }
                }
            }

            int resultLastErrorOffset = 0;
            ParserUnit result = null;
            int resultErrorsCount = 0;

            foreach (ParserUnit parseUnit in parseUnits)
            {
                parseUnit.AbortIfPossibly();
                parseUnit.Wait((int)LanguageParseTimeout.TotalMilliseconds / 4);

                if (!parseUnit.IsAlive && parseUnit.ParseTree == null) // Ignore languages with critical errors
                {
                    continue;
                }

                List<ParsingException> parseErrors = parseUnit.Errors.Where(error =>
                     error is ParsingException parsingException && parsingException.InnerException == null)
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
                    if (errorCount == resultErrorsCount && previousLanguage != Language.Uncertain)
                    {
                        result = new ParserUnit(previousLanguage, null);
                    }
                    else if (errorCount < resultErrorsCount)
                    {
                        resultErrorsCount = errorCount;
                        result = parseUnit;
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
