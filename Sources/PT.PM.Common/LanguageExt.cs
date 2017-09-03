using System;
using System.Collections.Generic;
using System.Linq;
using static PT.PM.Common.Language;

namespace PT.PM.Common
{
    public static class LanguageExt
    {
        private static readonly char[] LanguageSeparators = new char[] { ',', ' ', '\t', '\r', '\n' };

        public static readonly Dictionary<Language, LanguageInfo> LanguageInfos = new Dictionary<Language, LanguageInfo>()
        {
            [CSharp] = new LanguageInfo(CSharp, ".cs", false, "C#", haveAntlrParser: false),
            [Java] = new LanguageInfo(Java, ".java", false, "Java"),
            [Php] = new LanguageInfo(Php, new [] { ".php" }, true, "PHP", new [] { JavaScript, Html }),
            [PlSql] = new LanguageInfo(PlSql, new [] { ".sql", ".pks", ".pkb", ".tps", ".vw" }, true, "PL/SQL"),
            [TSql] = new LanguageInfo(TSql, ".sql", true, "T-SQL"),
            [Aspx] = new LanguageInfo(Aspx, new [] { ".asax", ".aspx", ".ascx", ".master" }, false, "Aspx", new [] { CSharp }),
            [JavaScript] = new LanguageInfo(JavaScript, ".js", false, "JavaScript"),
            [Html] = new LanguageInfo(Html, ".html", true, "HTML", new [] { JavaScript })
        };

        public static readonly Language[] AllLanguages = new Language[]
        {
            CSharp,
            Java,
            Php,
            PlSql,
            TSql,
            Aspx,
            JavaScript,
            Html
        };

        public static readonly Language[] AllGplPatternLanguages = new Language[]
        {
            CSharp,
            Java,
            Php,
            Java,
            JavaScript
        };

        public static readonly Language[] AllSqlPatternLanguages = new Language[]
        {
            PlSql,
            TSql
        };

        public static readonly Language[] AllPatternLanguages = new Language[]
        {
            CSharp,
            Java,
            Php,
            PlSql,
            TSql,
            JavaScript,
            Html
        };

        public static IEnumerable<string> AllExtensions => LanguageInfos.SelectMany(extLang => extLang.Value.Extensions);

        public static bool IsCaseInsensitive(this Language language)
        {
            return LanguageInfos[language].CaseInsensitive;
        }

        public static bool HaveAntlrParser(this Language language)
        {
            return LanguageInfos[language].HaveAntlrParser;
        }

        public static string[] GetExtensions(Language[] languages)
        {
            return languages.SelectMany(lang => GetExtensions(lang)).ToArray();
        }

        public static string[] GetExtensions(Language language)
        {
            return LanguageInfos[language].Extensions;
        }

        public static HashSet<Language> GetSelfAndSublanguages(this Language language)
        {
            var result = new HashSet<Language>();
            result.Add(language);
            foreach (var lang in LanguageInfos[language].Sublanguages)
                result.Add(lang);
            return result;
        }

        public static Language? GetLanguageFromFileName(string fileName)
        {
            var fileNameLower = fileName.ToLower();
            KeyValuePair<Language, LanguageInfo> result = LanguageInfos.FirstOrDefault(pair => pair.Value.Extensions.Any(
                ext => fileNameLower.Contains(ext)));
            if (result.Equals(default(KeyValuePair<Language, LanguageInfo>)))
            {
                return null;
            }
            else
            {
                return result.Key;
            }
        }

        public static Language[] ParseLanguages(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                var resultLanguages = new List<Language>();
                string[] languageStrings = str.Split(LanguageSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string languageString in languageStrings)
                {
                    Language language;
                    if (Enum.TryParse(languageString, true, out language))
                    {
                        resultLanguages.Add(language);
                    }
                }
                return resultLanguages.ToArray();
            }
            else
            {
                return AllPatternLanguages;
            }
        }
    }
}
