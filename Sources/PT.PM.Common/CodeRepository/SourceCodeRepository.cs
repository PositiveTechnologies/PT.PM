using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.Common.CodeRepository
{
    public abstract class SourceCodeRepository : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string RootPath { get; protected set; } = "";

        public HashSet<Language> Languages { get; set; } = new HashSet<Language>(LanguageUtils.LanguageInfos.Keys);

        public SerializationFormat? Format { get; }

        protected SourceCodeRepository(SerializationFormat? format)
        {
            Format = format;
        }

        public abstract IEnumerable<string> GetFileNames();

        public abstract IFile ReadFile(string fileName);

        public virtual bool IsFileIgnored(string fileName, bool withParser)
        {
            string fileExtension = System.IO.Path.GetExtension(fileName);

            if (Format == null)
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

            return !fileExtension.EndsWith(((SerializationFormat)Format).GetExtension(), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLanguageIgnored(Language language, string fileExtension, bool withParser)
        {
            if (language.GetExtensions().Any(ext => ext == fileExtension))
            {
                return withParser && !language.IsParserExists();
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
