using PT.PM.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM
{
    public abstract class LanguageDetector
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language? DetectIfRequired(string sourceCodeFileName)
        {
            return DetectIfRequired(sourceCodeFileName, File.ReadAllText(sourceCodeFileName));
        }

        public Language? DetectIfRequired(string sourceCodeFileName, string sourceCode, Language[] languages = null)
        {
            Language? result = null;
            if ((languages?.Length ?? 0) == 1)
            {
                result = languages[0];
            }
            else if (!string.IsNullOrEmpty(sourceCodeFileName))
            {
                string[] extensions = GetExtensions(sourceCodeFileName);
                Language[] finalLanguages = GetLanguagesIntersection(extensions, languages);
                if (finalLanguages.Length == 1 || finalLanguages.Contains(Language.CSharp))
                {
                    result = finalLanguages[0];
                }
                else if (finalLanguages.Length > 1)
                {
                    result = Detect(sourceCode, finalLanguages);
                    LogDetection(result, finalLanguages, sourceCodeFileName);
                }
            }
            else
            {
                result = Detect(sourceCode, languages);
                LogDetection(result, languages ?? LanguageExt.AllLanguages, sourceCodeFileName);
            }
            return result;
        }

        public abstract Language? Detect(string sourceCode, Language[] languages = null);

        protected void LogDetection(Language? detectedLanguage, Language[] languages, string sourceCodeFileName)
        {
            string languagesString = string.Join(", ", languages);
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

        private static Language[] GetLanguagesIntersection(string[] extensions, Language[] languages)
        {
            var result = new List<Language>();
            if (extensions.Length == 0)
            {
                return languages ?? new Language[0];
            }
            foreach (var extension in extensions)
            {
                var normalizedExtension = extension.ToLowerInvariant();
                foreach (var languageInfo in LanguageExt.LanguageInfos)
                {
                    var extensionIsFine = string.IsNullOrEmpty(normalizedExtension) || languageInfo.Value.Extensions.Contains(normalizedExtension);
                    var languageIsFine = languages == null || languages.Contains(languageInfo.Key);
                    if (extensionIsFine && languageIsFine)
                    {
                        result.Add(languageInfo.Key);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
