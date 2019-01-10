using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.CSharpParseTreeUst;
using PT.PM.PhpParseTreeUst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM
{
    public class ParserLanguageDetector : LanguageDetector
    {
        private readonly static Regex openTagRegex = new Regex("<\\w+>", RegexOptions.Compiled);
        private readonly static Regex closeTagRegex = new Regex("<\\/\\w+>", RegexOptions.Compiled);

        private Language previousLanguage;

        public TimeSpan LanguageParseTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan CheckParseResultTimeSpan { get; set; } = TimeSpan.FromMilliseconds(100);

        public override DetectionResult Detect(CodeFile codeFile, IEnumerable<Language> languages = null)
        {
            List<Language> langs = (languages ?? LanguageUtils.LanguagesWithParser).ToList();

            // Any PHP file contains start tag.
            if (!codeFile.Data.Contains("<?"))
            {
                langs.Remove(langs.FirstOrDefault(l => l == Php.Language));
            }
            // Aspx and html code contains at least one tag.
            if (!openTagRegex.IsMatch(codeFile.Data) || !closeTagRegex.IsMatch(codeFile.Data))
            {
                langs.Remove(langs.FirstOrDefault(l => l == Aspx.Language));
                langs.Remove(langs.FirstOrDefault(l => l == Html.Language));
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
                Thread thread = new Thread((object obj) =>
                {
                    ((ParserUnit)obj).Parse(codeFile);
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
                    if (parseUnit.ParseErrorCount == 0 && parseUnit.Language != Aspx.Language)
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
