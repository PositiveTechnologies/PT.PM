using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.CodeRepository
{
    public abstract class SourceCodeRepository : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string RootPath { get; protected set; } = "";

        public HashSet<Language> Languages { get; set; } = new HashSet<Language>(LanguageUtils.Languages.Values);

        public bool LoadJson { get; set; }

        public abstract IEnumerable<string> GetFileNames();

        public abstract CodeFile ReadFile(string fileName);

        public virtual bool IsFileIgnored(string fileName, bool withParser)
        {
            string fileExtension = System.IO.Path.GetExtension(fileName);

            if (!LoadJson)
            {
                foreach (Language language in Languages)
                {
                    bool ignored = IsLanguageIgnored(language, fileExtension, withParser);
                    if (!ignored)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return !fileExtension.EndsWith("json", StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool IsLanguageIgnored(Language language, string fileExtension, bool withParser)
        {
            if (language.Extensions.Any(ext => ext == fileExtension))
            {
                return withParser ? !language.IsParserExists() : false;
            }

            if (LanguageUtils.SuperLanguages.TryGetValue(language, out HashSet<Language> superLanguages))
            {
                foreach (Language superLanguage in superLanguages)
                {
                    bool ignored = IsLanguageIgnored(superLanguage, fileExtension, withParser);
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
