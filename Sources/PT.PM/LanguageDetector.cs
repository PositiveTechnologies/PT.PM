using PT.PM.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public int MaxStackSize { get; set; } = 0;

        public DetectionResult DetectIfRequired(string codeFileName)
        {
            var codeFile = new CodeFile(File.ReadAllText(codeFileName))
            {
                Name = codeFileName
            };
            return DetectIfRequired(codeFile);
        }

        public DetectionResult DetectIfRequired(CodeFile codeFile, IEnumerable<Language> languages = null)
        {
            DetectionResult result = null;

            if ((languages?.Count() ?? 0) == 1)
            {
                result = new DetectionResult(languages.First());
            }
            else if (!string.IsNullOrEmpty(codeFile.Name))
            {
                string[] extensions = GetExtensions(codeFile.Name);
                List<Language> finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Count == 1 || finalLanguages.Any(final => final.Key == "CSharp"))
                {
                    result = new DetectionResult(finalLanguages[0]);
                }
                else if (finalLanguages.Count > 1)
                {
                    result = Detect(codeFile.Code, finalLanguages);
                    LogDetection(result, finalLanguages, codeFile);
                }
            }
            else
            {
                result = Detect(codeFile.Code, languages);
                LogDetection(result, languages ?? LanguageUtils.Languages.Values, codeFile);
            }

            return result;
        }

        public abstract DetectionResult Detect(string sourceCode, IEnumerable<Language> languages = null);

        protected void LogDetection(DetectionResult detectionResult, IEnumerable<Language> languages, CodeFile codeFile)
        {
            string languagesString = string.Join(", ", languages.Select(lang => lang.Title));
            if (detectionResult != null)
            {
                Logger.LogDebug($"Language {detectionResult.Language} (from {languagesString}) has been detected for file \"{codeFile}\". ");
            }
            else
            {
                Logger.LogDebug($"Language has not been detected from ({languagesString}) for file \"{codeFile}\". ");
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
