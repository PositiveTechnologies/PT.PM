using PT.PM.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public DetectionResult DetectIfRequired(string sourceCodeFileName)
        {
            return DetectIfRequired(sourceCodeFileName, File.ReadAllText(sourceCodeFileName));
        }

        public DetectionResult DetectIfRequired(string sourceCodeFileName, string sourceCode, IEnumerable<Language> languages = null)
        {
            DetectionResult result = null;

            if ((languages?.Count() ?? 0) == 1)
            {
                result = new DetectionResult(languages.First());
            }
            else if (!string.IsNullOrEmpty(sourceCodeFileName))
            {
                string[] extensions = GetExtensions(sourceCodeFileName);
                List<Language> finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Count == 1 || finalLanguages.Any(final => final.Key == "CSharp"))
                {
                    result = new DetectionResult(finalLanguages[0]);
                }
                else if (finalLanguages.Count > 1)
                {
                    result = Detect(sourceCode, finalLanguages);
                    LogDetection(result, finalLanguages, sourceCodeFileName);
                }
            }
            else
            {
                result = Detect(sourceCode, languages);
                LogDetection(result, languages ?? LanguageUtils.Languages.Values, sourceCodeFileName);
            }

            return result;
        }

        public abstract DetectionResult Detect(string sourceCode, IEnumerable<Language> languages = null);

        protected void LogDetection(DetectionResult detectionResult, IEnumerable<Language> languages, string sourceCodeFileName)
        {
            string languagesString = string.Join(", ", languages.Select(lang => lang.Title));
            if (detectionResult != null)
            {
                Logger.LogDebug($"Language {detectionResult.Language} (from {languagesString}) has been detected for file \"{sourceCodeFileName}\". ");
            }
            else
            {
                Logger.LogDebug($"Language has not been recognized from ({languagesString}) for file \"{sourceCodeFileName}\". ");
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
                foreach (Language language in LanguageUtils.Languages.Values)
                {
                    var extensionIsFine = string.IsNullOrEmpty(normalizedExtension) ||
                        language.Extensions.Contains(normalizedExtension);
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
