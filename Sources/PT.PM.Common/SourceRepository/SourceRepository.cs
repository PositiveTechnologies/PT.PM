using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.Common.SourceRepository
{
    public abstract class SourceRepository : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string RootPath { get; protected set; } = "";

        public HashSet<Language> Languages { get; set; } = new HashSet<Language>(LanguageUtils.LanguageInfos.Keys);

        public abstract IEnumerable<string> GetFileNames();

        public abstract IFile ReadFile(string fileName);

        public virtual bool IsFileIgnored(string fileName, bool withParser, out Language language)
        {
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            foreach (Language l in Languages)
            {
                bool ignored = IsLanguageIgnored(l, fileName, fileExtension, withParser);
                if (!ignored)
                {
                    language = l;
                    return false;
                }
            }

            language = Language.Uncertain;
            return true;
        }

        private bool IsLanguageIgnored(Language language, string fileName, string fileExtension, bool withParser)
        {
            if (language.GetExtensions().Any(ext => ext == fileExtension))
            {
                if (language.IsSerialization())
                {
                    string secondExt = Path.GetExtension(Path.GetFileNameWithoutExtension(fileName)).ToLowerInvariant();
                    return secondExt != ".ust" && secondExt != ".cpg";
                }

                return withParser && !language.IsParserExists();
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
