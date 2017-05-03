using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common
{
    public static class LanguageExt
    {
        private static char[] languageFlagSeparators = new char[] { ',', ' ', '\t', '\r', '\n' };

        public static readonly Dictionary<Language, LanguageInfo> LanguageInfos = new Dictionary<Language, LanguageInfo>()
        {
            [Language.CSharp] = new LanguageInfo(Language.CSharp, ".cs", false, "C#", haveAntlrParser: false),
            [Language.Java] = new LanguageInfo(Language.Java, ".java", false, "Java"),
            [Language.Php] = new LanguageInfo(Language.Php, new string[] { ".php" }, true, "PHP", LanguageFlags.JavaScript | LanguageFlags.Html),
            [Language.PlSql] = new LanguageInfo(Language.PlSql, ".sql", true, "PL/SQL"),
            [Language.TSql] = new LanguageInfo(Language.TSql, ".sql", true, "T-SQL"),
            [Language.Aspx] = new LanguageInfo(Language.Aspx, new string[] { ".asax", ".aspx", ".ascx", ".master" }, false, "Aspx", LanguageFlags.CSharp),
            [Language.JavaScript] = new LanguageInfo(Language.JavaScript, ".js", false, "JavaScript"),
            [Language.Html] = new LanguageInfo(Language.Html, ".html", true, "HTML", LanguageFlags.JavaScript)
        };

        public static readonly LanguageFlags AllLanguages = LanguageFlags.CSharp | LanguageFlags.Java | LanguageFlags.Php | LanguageFlags.PlSql | LanguageFlags.TSql | LanguageFlags.Aspx | LanguageFlags.JavaScript | LanguageFlags.Html;

        public static readonly LanguageFlags AllGplPatternLanguages = LanguageFlags.CSharp | LanguageFlags.Java | LanguageFlags.Php | LanguageFlags.Java | LanguageFlags.JavaScript;
        public static readonly LanguageFlags AllSqlPatternLanguages = LanguageFlags.PlSql | LanguageFlags.TSql;
        public static readonly LanguageFlags AllPatternLanguages = LanguageFlags.CSharp | LanguageFlags.Java | LanguageFlags.Php | LanguageFlags.PlSql | LanguageFlags.TSql | LanguageFlags.JavaScript | LanguageFlags.Html;

        public static IEnumerable<Language> Languages => LanguageInfos.Keys;

        public static IEnumerable<string> Extensions => LanguageInfos.SelectMany(extLang => extLang.Value.Extensions);

        public static bool IsCaseInsensitive(this Language language)
        {
            return LanguageInfos[language].CaseInsensitive;
        }

        public static bool HaveAntlrParser(this Language language)
        {
            return LanguageInfos[language].HaveAntlrParser;
        }

        public static bool IsCaseInsensitive(this LanguageFlags languageFlags)
        {
            int flags = (int)languageFlags;
            int language = 0;
            while (flags != 0)
            {
                if ((flags & 1) == 1)
                {
                    var isCaseInsensitive = ((Language)language).IsCaseInsensitive();
                    if (!isCaseInsensitive)
                    {
                        return false;
                    }
                }

                flags = flags >> 1;
                ++language;
            }
            return true;
        }

        public static LanguageFlags GetLanguageWithDependentLanguages(this Language language)
        {
            return language.ToFlags() | LanguageInfos[language].DependentLanguages;
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

        public static string[] GetExtensions(Language language)
        {
            return LanguageInfos[language].Extensions;
        }

        public static string[] GetExtensions(LanguageFlags languages)
        {
            return languages.GetLanguages().SelectMany(lang => GetExtensions(lang)).ToArray();
        }

        public static Language[] GetImpactLanguages(this LanguageFlags value)
        {
            var languages = value.GetLanguages();
            var result = new List<Language>();
            foreach (Language language in languages)
            {
                var impactLanguageInfo = LanguageInfos.FirstOrDefault(languageInfo => languageInfo.Value.DependentLanguages.Is(language));
                if (!impactLanguageInfo.Equals(default(KeyValuePair<Language, LanguageInfo>)))
                {
                    if (!result.Contains(impactLanguageInfo.Key))
                    {
                        result.Add(impactLanguageInfo.Key);
                    }
                }
            }

            return result.ToArray();
        }

        public static Language[] GetLanguages(this LanguageFlags value)
        {
            var result = new List<Language>();
            foreach (Language language in Languages)
            {
                if (value.Is(language))
                {
                    result.Add(language);
                }
            }
            return result.ToArray();
        }

        public static bool Is(this LanguageFlags value, Language language) => value.Is(language.ToFlags());

        public static bool Is(this LanguageFlags value, LanguageFlags flag) => (value & flag) == flag;

        public static LanguageFlags ToFlags(this Language language)
        {
            return (LanguageFlags)(1 << (int)language);
        }

        public static LanguageFlags ParseLanguages(string languageFlagsString)
        {
            LanguageFlags resultLanguages;
            if (languageFlagsString != null)
            {
                resultLanguages = LanguageFlags.None;
                string[] languageStrings = languageFlagsString.Split(languageFlagSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string languageString in languageStrings)
                {
                    Language language;
                    if (Enum.TryParse(languageString, true, out language))
                    {
                        resultLanguages |= language.ToFlags();
                    }
                }
            }
            else
            {
                resultLanguages = LanguageExt.AllPatternLanguages;
            }

            return resultLanguages;
        }
    }
}
