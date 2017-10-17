using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.CodeRepository
{
    public abstract class SourceCodeRepository : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string RootPath { get; protected set; } = "";

        public HashSet<Language> Languages { get; set; } = new HashSet<Language>(LanguageUtils.Languages.Values);

        public abstract IEnumerable<string> GetFileNames();

        public abstract SourceCodeFile ReadFile(string fileName);

        public virtual bool IsFileIgnored(string fileName)
        {
            string fileExtension = System.IO.Path.GetExtension(fileName);

            foreach (Language language in Languages)
            {
                bool ignored = IsLanguageIgnored(language, fileExtension);
                if (!ignored)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsLanguageIgnored(Language language, string fileExtension)
        {
            if (language.Extensions.Any(ext => ext == fileExtension))
            {
                return false;
            }

            HashSet<Language> superLanguages;
            if (LanguageUtils.SuperLanguages.TryGetValue(language, out superLanguages))
            {
                foreach (Language superLanguage in superLanguages)
                {
                    bool ignored = IsLanguageIgnored(superLanguage, fileExtension);
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
