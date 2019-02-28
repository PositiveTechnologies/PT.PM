using System;
using PT.PM.Common;
using PT.PM.Common.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common.Files;
using PT.PM.LanguageDetectors;

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

        public DetectionResult DetectIfRequired(TextFile sourceFile, out TimeSpan detectionTimeSpan, ICollection<Language> languages = null)
        {
            DetectionResult result = null;

            if ((languages?.Count ?? 0) == 1)
            {
                result = new DetectionResult(languages.First());
            }
            else if (!string.IsNullOrEmpty(sourceFile.Name))
            {
                string[] extensions = GetExtensions(sourceFile.Name);
                List<Language> finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Count == 1 || finalLanguages.Any(final => final == Language.CSharp))
                {
                    result = new DetectionResult(finalLanguages[0]);
                }
                else if (finalLanguages.Count > 1)
                {
                    if (finalLanguages.Count == 2 && finalLanguages.Contains(Language.Html) && finalLanguages.Contains(Language.Php))
                    {
                        result = new DetectionResult(Language.Php);
                    }
                    else
                    {
                        var stopwatch = Stopwatch.StartNew();

                        if (finalLanguages.Any(lang => lang.IsSql()))
                        {
                            List<Language> sqls = SqlDialectDetector.Detect(sourceFile);

                            Logger.LogInfo($"File: {sourceFile}; Sql detection: {string.Join(", ", sqls)}");

                            if (sqls.Count == 1)
                            {
                                stopwatch.Stop();
                                detectionTimeSpan = stopwatch.Elapsed;
                                return new DetectionResult(sqls[0]);
                            }

                            finalLanguages.RemoveAll(lang => lang.IsSql());
                            finalLanguages.AddRange(sqls);
                        }

                        ParserLanguageDetector.MaxStackSize = MaxStackSize;
                        result = ParserLanguageDetector.Detect(sourceFile, previousLanguage, finalLanguages);
                        previousLanguage = result.Language;

                        stopwatch.Stop();
                        detectionTimeSpan = stopwatch.Elapsed;
                    }
                }
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                ParserLanguageDetector.MaxStackSize = MaxStackSize;
                result = ParserLanguageDetector.Detect(sourceFile, previousLanguage, languages);
                previousLanguage = result.Language;
                stopwatch.Stop();
                detectionTimeSpan = stopwatch.Elapsed;
            }

            return result;
        }

        private static string[] GetExtensions(string fileName)
        {
            var result = new List<string>();
            string extension = Path.GetExtension(fileName);
            while (extension != "")
            {
                result.Add(extension);
                fileName = Path.GetFileNameWithoutExtension(fileName);
                extension = Path.GetExtension(fileName);
            }
            return result.ToArray();
        }

        private static List<Language> GetLanguagesIntersection(string[] extensions, IEnumerable<Language> languages)
        {
            var result = new List<Language>();
            if (extensions.Length == 0)
            {
                return languages.ToList();
            }
            foreach (var extension in extensions)
            {
                var normalizedExtension = extension.ToLowerInvariant();
                foreach (Language language in LanguageUtils.Languages)
                {
                    var extensionIsFine = string.IsNullOrEmpty(normalizedExtension) ||
                        language.GetExtensions().Contains(normalizedExtension);
                    var languageIsFine = languages == null || languages.Contains(language);
                    if (extensionIsFine && languageIsFine)
                    {
                        result.Add(language);
                    }
                }
            }
            return result;
        }
    }
}
