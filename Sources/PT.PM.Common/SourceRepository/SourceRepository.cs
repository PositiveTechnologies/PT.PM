using PT.PM.Common.Files;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Common.SourceRepository
{
    public abstract class SourceRepository : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string RootPath { get; protected set; } = "";

        public HashSet<Language> Languages { get; set; } = new HashSet<Language>(LanguageUtils.LanguageInfos.Keys);

        public abstract IEnumerable<string> GetFileNames();

        public abstract IFile ReadFile(string fileName);

        public virtual Language[] GetLanguages(string fileName, bool withParser)
        {
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            var result = new List<Language>();

            foreach (Language language in Languages)
            {
                bool ignored = IsLanguageIgnored(language, fileName, fileExtension, withParser);
                if (!ignored)
                {
                    result.Add(language);
                }
            }

            return result.ToArray();
        }

        private bool IsLanguageIgnored(Language language, string fileName, string fileExtension, bool withParser)
        {
            if (language.GetExtensions().Any(ext => ext == fileExtension))
            {
                if (language.IsSerializing())
                {
                    string secondExt = Path.GetExtension(Path.GetFileNameWithoutExtension(fileName)).ToLowerInvariant();
                    return secondExt != ".ust" && secondExt != ".cpg";
                }

                return withParser && !language.IsParserExistsOrSerializing();
            }

            if (LanguageUtils.SuperLanguages.TryGetValue(language, out HashSet<Language> superLanguages))
            {
                foreach (Language superLanguage in superLanguages)
                {
                    bool ignored = IsLanguageIgnored(superLanguage, fileName, fileExtension, withParser);
                    if (!ignored)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
