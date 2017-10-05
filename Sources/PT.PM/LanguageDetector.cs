using PT.PM.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public LanguageInfo DetectIfRequired(string sourceCodeFileName)
        {
            return DetectIfRequired(sourceCodeFileName, File.ReadAllText(sourceCodeFileName));
        }

        public LanguageInfo DetectIfRequired(string sourceCodeFileName, string sourceCode, IEnumerable<LanguageInfo> languages = null)
        {
            LanguageInfo result = null;
            if ((languages?.Count() ?? 0) == 1)
            {
                result = languages.First();
            }
            else if (!string.IsNullOrEmpty(sourceCodeFileName))
            {
                string[] extensions = GetExtensions(sourceCodeFileName);
                List<LanguageInfo> finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Count == 1 || finalLanguages.Any(final => final.Key == "CSharp"))
                {
                    result = finalLanguages[0];
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

        public abstract LanguageInfo Detect(string sourceCode, IEnumerable<LanguageInfo> languages = null);

        protected void LogDetection(LanguageInfo detectedLanguage, IEnumerable<LanguageInfo> languages, string sourceCodeFileName)
        {
            string languagesString = string.Join(", ", languages.Select(lang => lang.Title));
            if (detectedLanguage != null)
            {
                Logger.LogDebug($"Language {detectedLanguage} (from {languagesString}) has been detected for file \"{sourceCodeFileName}\". ");
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

        private static List<LanguageInfo> GetLanguagesIntersection(string[] extensions, IEnumerable<LanguageInfo> languages)
        {
            var result = new List<LanguageInfo>();
            if (extensions.Length == 0)
            {
                return languages.ToList() ?? new List<LanguageInfo>();
            }
            foreach (var extension in extensions)
            {
                var normalizedExtension = extension.ToLowerInvariant();
                foreach (LanguageInfo languageInfo in LanguageUtils.Languages.Values)
                {
                    var extensionIsFine = string.IsNullOrEmpty(normalizedExtension) || languageInfo.Extensions.Contains(normalizedExtension);
                    var languageIsFine = languages == null || languages.Contains(languageInfo);
                    if (extensionIsFine && languageIsFine)
                    {
                        result.Add(languageInfo);
                    }
                }
            }
            return result;
        }
    }
}
