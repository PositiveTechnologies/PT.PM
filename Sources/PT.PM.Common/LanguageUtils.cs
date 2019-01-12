using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.Common
{
    public static class LanguageUtils
    {
        private static readonly string[] LanguageSeparators = new string[] { " ", ",", ";", "|" };

        private static Dictionary<Language, Type> parsers;
        private static Dictionary<Language, Type> converters;

        public static readonly Dictionary<string, Language> Languages;
        public readonly static Dictionary<string, Language> PatternLanguages;
        public static readonly Dictionary<string, Language> SqlLanguages;
        public static readonly Dictionary<Language, HashSet<Language>> SuperLanguages;
        public static readonly HashSet<Language> LanguagesWithParser;

        static LanguageUtils()
        {
            parsers = new Dictionary<Language, Type>();
            converters = new Dictionary<Language, Type>();

            Languages = new Dictionary<string, Language>();
            PatternLanguages = new Dictionary<string, Language>();
            SqlLanguages = new Dictionary<string, Language>();
            SuperLanguages = new Dictionary<Language, HashSet<Language>>();
            LanguagesWithParser = new HashSet<Language>();

            var subParsers = new Dictionary<Language, Type>();
            var subConverters = new Dictionary<Language, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var visitedAssemblies = new List<Assembly>();

            foreach (Assembly assembly in assemblies)
            {
                LoadAndProcessReferencedAssembly(assembly);

                void LoadAndProcessReferencedAssembly(Assembly localAssembly)
                {
                    ProcessAssembly(localAssembly, subParsers, subConverters, visitedAssemblies);
                    if (!localAssembly.IsActual())
                    {
                        return;
                    }

                    foreach (AssemblyName assemblyName in localAssembly.GetReferencedAssemblies())
                    {
                        if (assemblies.All(a => a.FullName != assemblyName.FullName))
                        {
                            LoadAndProcessReferencedAssembly(Assembly.Load(assemblyName));
                        }
                    }
                }

                ProcessAssembly(assembly, subParsers, subConverters, visitedAssemblies);
            }

            foreach (var subParser in subParsers)
            {
                if (!parsers.ContainsKey(subParser.Key))
                {
                    parsers.Add(subParser.Key, subParser.Value);
                }
            }

            foreach (var subConverter in subConverters)
            {
                if (!converters.ContainsKey(subConverter.Key))
                {
                    converters.Add(subConverter.Key, subConverter.Value);
                }
            }

            foreach (var parser in parsers)
            {
                LanguagesWithParser.Add(parser.Key);
            }
        }

        public static bool IsParserExists(this Language language) => LanguagesWithParser.Contains(language);

        public static ILanguageParser CreateParser(this Language language)
        {
            if (parsers.TryGetValue(language, out Type parserType))
            {
                return (ILanguageParser)Activator.CreateInstance(parserType);
            }
            else
            {
                throw new NotImplementedException($"Language {language} parser is not supported");
            }
        }

        public static IParseTreeToUstConverter CreateConverter(this Language language)
        {
            if (converters.TryGetValue(language, out Type converterType))
            {
                return (IParseTreeToUstConverter)Activator.CreateInstance(converterType);
            }
            else
            {
                throw new NotImplementedException($"Language {language} converter is not supported");
            }
        }

        public static HashSet<Language> ParseLanguages(this string languages, bool allByDefault = true,
            bool patternLanguages = false)
        {
            if (languages == null)
            {
                return new HashSet<Language>(!patternLanguages ? Languages.Values : PatternLanguages.Values);
            }

            return languages.Split(LanguageSeparators, StringSplitOptions.RemoveEmptyEntries).ParseLanguages(allByDefault, patternLanguages);
        }

        public static HashSet<Language> ParseLanguages(this IEnumerable<string> languageStrings, bool allByDefault = true,
            bool patternLanguages = false)
        {
            string[] languageStringsArray = languageStrings?.ToArray() ?? ArrayUtils<string>.EmptyArray;
            var languages = !patternLanguages ? Languages.Values : PatternLanguages.Values;
            HashSet<Language> negationLangs = new HashSet<Language>(languages);

            if (allByDefault && languageStringsArray.Length == 0)
            {
                return negationLangs;
            }

            var langs = new HashSet<Language>();
            bool containsNegation = false;
            foreach (string languageString in languageStringsArray)
            {
                bool negation = false;
                string langStr = languageString;
                if (langStr.StartsWith("~") || langStr.StartsWith("!"))
                {
                    negation = true;
                    langStr = langStr.Substring(1);
                }
                bool isSql = langStr.EqualsIgnoreCase("sql");

                foreach (Language language in languages)
                {
                    bool result = isSql
                        ? language.IsSql
                        : (language.Key.EqualsIgnoreCase(langStr) || language.Title.EqualsIgnoreCase(langStr) ||
                           language.Extensions.Any(ext => (ext.StartsWith(".") ? ext.Substring(1) : ext).EqualsIgnoreCase(langStr)));
                    if (negation)
                    {
                        containsNegation = true;
                        if (result)
                        {
                            negationLangs.Remove(language);
                        }
                    }
                    else
                    {
                        if (result)
                        {
                            langs.Add(language);
                        }
                    }
                };
            }

            if (containsNegation)
            {
                foreach (Language lang in negationLangs)
                {
                    langs.Add(lang);
                }
            }

            return langs;
        }

        public static HashSet<Language> GetSelfAndSublanguages(this Language language)
        {
            var result = new HashSet<Language> { language };
            foreach (Language lang in language.Sublanguages)
                result.Add(lang);
            return result;
        }

        private static void ProcessAssembly(Assembly assembly,
            Dictionary<Language, Type> subParsers, Dictionary<Language, Type> subConverters,
            List<Assembly> visitedAssemblies)
        {
            if (!assembly.IsActual() || visitedAssemblies.Contains(assembly))
            {
                return;
            }
            visitedAssemblies.Add(assembly);

            foreach (Type type in assembly.GetTypes().Where(type => type.IsClass))
            {
                if (type.IsAbstract)
                {
                    var languageFields = type.GetFields()
                        .Where(prop => prop.Attributes.HasFlag(FieldAttributes.Static | FieldAttributes.InitOnly)
                        && prop.FieldType == typeof(Language));
                    foreach (FieldInfo languageField in languageFields)
                    {
                        ProcessLanguage((Language)languageField.GetValue(null));
                    }
                }
                else
                {
                    var interfaces = type.GetInterfaces();
                    if (interfaces.Contains(typeof(ILanguageParser)))
                    {
                        var parser = (ILanguageParser)Activator.CreateInstance(type);
                        parsers.Add(parser.Language, parser.GetType());
                        foreach (Language sublanguage in parser.Language.Sublanguages)
                        {
                            subParsers.Add(sublanguage, type);
                        };
                    }
                    else if (interfaces.Contains(typeof(IParseTreeToUstConverter)))
                    {
                        var converter = (IParseTreeToUstConverter)Activator.CreateInstance(type);
                        converters.Add(converter.Language, converter.GetType());
                        foreach (Language sublanguage in converter.Language.Sublanguages)
                        {
                            subConverters.Add(sublanguage, type);
                        }
                    }
                }
            }
        }

        private static void ProcessLanguage(Language language)
        {
            string languageKey = language.Key;

            if (Languages.ContainsKey(languageKey))
            {
                return;
            }

            foreach (Language Sublanguage in language.Sublanguages)
            {
                ProcessLanguage(Sublanguage);
            }

            Languages.Add(languageKey, language);
            if (language.IsPattern)
            {
                PatternLanguages.Add(languageKey, language);
            }
            if (language.IsSql)
            {
                SqlLanguages.Add(languageKey, language);
            }

            foreach (Language sublanguage in language.Sublanguages)
            {
                if (!SuperLanguages.TryGetValue(sublanguage, out HashSet<Language> superLanguages))
                {
                    superLanguages = new HashSet<Language>();
                    SuperLanguages.Add(sublanguage, superLanguages);
                }

                superLanguages.Add(language);
            }
        }
    }
}
