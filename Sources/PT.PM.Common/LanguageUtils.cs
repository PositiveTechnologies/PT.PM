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

        public readonly static Dictionary<string, Language> Languages;
        public readonly static Dictionary<string, Language> PatternLanguages;
        public readonly static Dictionary<string, Language> SqlLanguages;
        public readonly static Dictionary<Language, HashSet<Language>> SuperLanguages;

        static LanguageUtils()
        {
            parsers = new Dictionary<Language, Type>();
            converters = new Dictionary<Language, Type>();
            Languages = new Dictionary<string, Language>();
            PatternLanguages = new Dictionary<string, Language>();
            SqlLanguages = new Dictionary<string, Language>();
            SuperLanguages = new Dictionary<Language, HashSet<Language>>();

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
                        if (!assemblies.Any(a => a.FullName == assemblyName.FullName))
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

            foreach (Type type in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract))
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Contains(typeof(ILanguageParser)))
                {
                    var parser = (ILanguageParser)Activator.CreateInstance(type);
                    parsers.Add(parser.Language, parser.GetType());
                    foreach (Language sublanguage in parser.Language.Sublanguages)
                    {
                        subParsers.Add(sublanguage, type);
                    }

                    ProcessLanguage(new[] { parser.Language });
                    ProcessLanguage(parser.Language.Sublanguages);
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

        private static void ProcessLanguage(IEnumerable<Language> languages)
        {
            foreach (Language lang in languages)
            {
                string languageKey = lang.Key;
                Languages[languageKey] = lang;
                if (lang.IsPattern)
                {
                    PatternLanguages[languageKey] = lang;
                }
                if (lang.IsSql)
                {
                    SqlLanguages[languageKey] = lang;
                }
                
                foreach (Language sublanguage in lang.Sublanguages)
                {
                    HashSet<Language> superLanguages;
                    if (!SuperLanguages.TryGetValue(sublanguage, out superLanguages))
                    {
                        superLanguages = new HashSet<Language>();
                        SuperLanguages.Add(sublanguage, superLanguages);
                    }

                    superLanguages.Add(lang);
                }
            }
        }

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

        public static HashSet<Language> ToLanguages(this string languages)
        {
            return languages.Split(LanguageSeparators, StringSplitOptions.RemoveEmptyEntries).ToLanguages();
        }

        public static HashSet<Language> ToLanguages(this IEnumerable<string> languageStrings)
        {
            var langs = new HashSet<Language>();
            var negationLangs = Languages.Values.ToList();
            bool containsNegation = false;
            foreach (string languageString in languageStrings)
            {
                bool negation = false;
                string langStr = languageString;
                if (langStr.StartsWith("~") || langStr.StartsWith("!"))
                {
                    negation = true;
                    langStr = langStr.Substring(1);
                }
                bool isSql = langStr.Equals("sql", StringComparison.OrdinalIgnoreCase);

                foreach (Language language in Languages.Values)
                {
                    bool result = isSql
                        ? language.IsSql
                        : string.Equals(language.Key, langStr, StringComparison.OrdinalIgnoreCase);
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
            var result = new HashSet<Language>();
            result.Add(language);
            foreach (var lang in language.Sublanguages)
                result.Add(lang);
            return result;
        }
    }
}
