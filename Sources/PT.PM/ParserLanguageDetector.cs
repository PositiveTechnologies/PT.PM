using PT.PM.Common;
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

        public TimeSpan LanguageParseTimeout = TimeSpan.FromSeconds(20);

        public TimeSpan CheckParseResultTimeSpan = TimeSpan.FromMilliseconds(50);

        public override Language Detect(string sourceCode, IEnumerable<Language> languages = null)
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
                return langs[0];
            }

            foreach (Language language in langs)
            {
                ILanguageParser languageParser = language.CreateParser();

                Thread thread = new Thread((object obj) =>
                {
                    var languageParser2 = (ILanguageParser)obj;
                    languageParser2.Logger = new LoggerMessageCounter();
                    languageParser2.Parse(sourceCodeFile);
                });
                thread.IsBackground = true;
                thread.Start(languageParser);

                parseUnits.Enqueue(new ParserUnit(language, languageParser, thread));
            }

            int checkParseResultMs = CheckParseResultTimeSpan.Milliseconds;
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

            int minErrorCount = int.MaxValue;
            Language resultWithMinErrors = null;

            foreach (ParserUnit parseUnit in parseUnits)
            {
                parseUnit.Abort();

                int errorCount = parseUnit.ParseErrorCount;
                if (errorCount < minErrorCount)
                {
                    minErrorCount = errorCount;
                    resultWithMinErrors = parseUnit.Language;
                }
            }

            return resultWithMinErrors;
        }
    }
}
