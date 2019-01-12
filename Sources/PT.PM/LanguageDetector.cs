using PT.PM.Common;
using PT.PM.Common.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public int MaxStackSize { get; set; } = 0;

        public DetectionResult DetectIfRequired(string sourceFileName)
        {
            var sourceFile = new TextFile(FileExt.ReadAllText(sourceFileName))
            {
                Name = sourceFileName
            };
            return DetectIfRequired(sourceFile);
        }

        public DetectionResult DetectIfRequired(TextFile sourceFile, IEnumerable<Language> languages = null)
        {
            DetectionResult result = null;

            if ((languages?.Count() ?? 0) == 1)
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
                        result = Detect(sourceFile, finalLanguages);
                        LogDetection(result, finalLanguages, sourceFile);
                    }
                }
            }
            else
            {
                result = Detect(sourceFile, languages);
                LogDetection(result, languages ?? LanguageUtils.Languages, sourceFile);
            }

            return result;
        }

        public abstract DetectionResult Detect(TextFile sourceFile, IEnumerable<Language> languages = null);

        protected void LogDetection(DetectionResult detectionResult, IEnumerable<Language> languages, TextFile sourceFile)
        {
            string languagesString = string.Join(", ", languages.Select(lang => LanguageUtils.LanguageInfos[lang].Title));
            if (detectionResult != null)
            {
                Logger.LogDebug($"Language {detectionResult.Language} (from {languagesString}) detected for file \"{sourceFile}\". ");
            }
            else
            {
                Logger.LogDebug($"Language is not detected from ({languagesString}) for file \"{sourceFile}\". ");
            }
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
                return languages.ToList() ?? new List<Language>();
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
