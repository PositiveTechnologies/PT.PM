using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.Common
{
    public static class LanguageUtils
    {
        private static Dictionary<LanguageInfo, Type> parsers;
        private static Dictionary<LanguageInfo, Type> converters;

        public readonly static Dictionary<string, LanguageInfo> Languages;
        public readonly static Dictionary<string, LanguageInfo> PatternLanguages;
        public readonly static Dictionary<string, LanguageInfo> SqlLanguages;

        static LanguageUtils()
        {
            parsers = new Dictionary<LanguageInfo, Type>();
            converters = new Dictionary<LanguageInfo, Type>();
            Languages = new Dictionary<string, LanguageInfo>();
            PatternLanguages = new Dictionary<string, LanguageInfo>();
            SqlLanguages = new Dictionary<string, LanguageInfo>();

            var sublanguageParsers = new Dictionary<LanguageInfo, Type>();
            var sublanguageConverters = new Dictionary<LanguageInfo, Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (assembly.IsAssemblyActual())
                {
                    foreach (Type type in assembly.GetTypes()
                         .Where(type => type.IsClass && !type.IsAbstract))
                    {
                        var interfaces = type.GetInterfaces();
                        if (interfaces.Contains(typeof(ILanguageParser)))
                        {
                            var parser = (ILanguageParser)Activator.CreateInstance(type);
                            parsers.Add(parser.Language, parser.GetType());
                            foreach (LanguageInfo sublanguage in parser.Language.Sublanguages)
                            {
                                sublanguageParsers.Add(sublanguage, type);
                            }

                            ProcessLanguage(new[] { parser.Language });
                            ProcessLanguage(parser.Language.Sublanguages);
                        }
                        else if (interfaces.Contains(typeof(IParseTreeToUstConverter)))
                        {
                            var converter = (IParseTreeToUstConverter)Activator.CreateInstance(type);
                            converters.Add(converter.Language, converter.GetType());
                            foreach (LanguageInfo sublanguage in converter.Language.Sublanguages)
                            {
                                sublanguageConverters.Add(sublanguage, type);
                            }
                        }
                    }
                }
            }

            foreach (var subParser in sublanguageParsers)
            {
                if (!parsers.ContainsKey(subParser.Key))
                {
                    parsers.Add(subParser.Key, subParser.Value);
                }
            }

            foreach (var subConverter in sublanguageConverters)
            {
                if (!converters.ContainsKey(subConverter.Key))
                {
                    converters.Add(subConverter.Key, subConverter.Value);
                }
            }
        }

        private static void ProcessLanguage(IEnumerable<LanguageInfo> languages)
        {
            foreach (LanguageInfo lang in languages)
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
            }
        }

        public static ILanguageParser CreateParser(this LanguageInfo language)
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

        public static IParseTreeToUstConverter CreateConverter(this LanguageInfo language)
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

        public static List<LanguageInfo> ToLanguages(this IEnumerable<string> languages, ILogger logger)
        {
            var result = new List<LanguageInfo>();
            foreach (string langStr in languages)
            {
                LanguageInfo value;
                if ((value = Languages.Values.FirstOrDefault(lang =>
                    string.Equals(lang.Key, langStr, StringComparison.InvariantCultureIgnoreCase))) != null)
                {
                    result.Add(value);
                }
                else
                {
                    //logger.LogError(new ConversionException("", message: $"Language \"{langStr}\" is not supported or wrong"));
                }
            }
            return result;
        }

        public static HashSet<LanguageInfo> GetSelfAndSublanguages(this LanguageInfo language)
        {
            var result = new HashSet<LanguageInfo>();
            result.Add(language);
            foreach (var lang in language.Sublanguages)
                result.Add(lang);
            return result;
        }
    }
}
