using System;
using PT.PM.Common;
using PT.PM.Common.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM
{
    public class LanguageDetector : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public int MaxStackSize { get; set; }

        private Language previousLanguage = Language.Uncertain;

        public DetectionResult DetectIfRequired(string sourceFileName, out TimeSpan detectionTimeSpan)
        {
            var sourceFile = new TextFile(FileExt.ReadAllText(sourceFileName))
            {
                Name = sourceFileName
            };
            return DetectIfRequired(sourceFile, out detectionTimeSpan);
        }

        public DetectionResult DetectIfRequired(TextFile sourceFile, out TimeSpan detectionTimeSpan)
        {
            DetectionResult result = null;

            List<Language> languages = GetLanguages(sourceFile.Name);
            if (languages.Count == 1 || languages.Any(language => language == Language.CSharp))
            {
                result = new DetectionResult(languages[0]);
            }
            else if (languages.Count > 1)
            {
                if (languages.Count == 2 && languages.Contains(Language.Html) && languages.Contains(Language.Php))
                {
                    result = new DetectionResult(Language.Php);
                }
                else
                {
                    var stopwatch = Stopwatch.StartNew();

                    if (languages.Any(lang => lang.IsSql()))
                    {
                        List<Language> sqls = SqlDialectDetector.Detect(sourceFile);

                        Logger.LogInfo($"File: {sourceFile}; Sql detection: {string.Join(", ", sqls)}");

                        if (sqls.Count == 1)
                        {
                            stopwatch.Stop();
                            detectionTimeSpan = stopwatch.Elapsed;
                            return new DetectionResult(sqls[0]);
                        }

                        languages.RemoveAll(lang => lang.IsSql());
                        languages.AddRange(sqls);
                    }

                    ParserLanguageDetector.MaxStackSize = MaxStackSize;
                    result = ParserLanguageDetector.Detect(sourceFile, previousLanguage, languages);

                    if (result != null)
                    {
                        previousLanguage = result.Language;
                    }

                    stopwatch.Stop();
                    detectionTimeSpan = stopwatch.Elapsed;
                }
            }

            return result;
        }

        private static List<Language> GetLanguages(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return LanguageUtils.LanguagesWithParser.ToList();
            }

            var result = new List<Language>(LanguageUtils.Languages.Count);

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            foreach (Language language in LanguageUtils.Languages)
            {
                if (language.GetExtensions().Contains(extension))
                {
                    result.Add(language);
                }
            }

            return result;
        }
    }
}
