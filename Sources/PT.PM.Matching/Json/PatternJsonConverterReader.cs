using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Json;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using PT.PM.Common.Files;

namespace PT.PM.Matching.Json
{
    public class PatternJsonConverterReader : UstJsonConverterReader
    {
        private PatternRoot root;
        private Stack<PatternUst> ancestors = new Stack<PatternUst>();

        public string DefaultDataFormat { get; set; } = "Json";

        public string DefaultKey { get; set; } = "";

        public string DefaultFilenameWildcard { get; set; } = "";

        public HashSet<Language> DefaultLanguages { get; set; } = new HashSet<Language>(LanguageUtils.PatternLanguages.Values);

        public PatternJsonConverterReader(TextFile serializedFile)
            : base(serializedFile)
        {
        }

        public override bool CanConvert(Type objectType) => objectType.CanConvert();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jObject = JObject.Load(reader);

            object target = null;
            if (objectType == typeof(PatternRoot))
            {
                HashSet<Language> resultLanguages = ((string)jObject[nameof(PatternDto.Languages)])?.ParseLanguages(patternLanguages: true)
                    ?? DefaultLanguages;

                root = new PatternRoot
                {
                    Key = (string)jObject[nameof(PatternRoot.Key)] ?? DefaultKey,
                    FilenameWildcard = (string)jObject[nameof(PatternRoot.FilenameWildcard)] ?? DefaultFilenameWildcard,
                    Languages = resultLanguages,
                    DataFormat = (string)jObject[nameof(PatternRoot.DataFormat)] ?? DefaultDataFormat,
                    File = jObject[nameof(PatternRoot.File)]?.ToObject<TextFile>(serializer) ?? TextFile.Empty,
                };

                target = root;
                root.Node = jObject[nameof(PatternRoot.Node)].ToObject<PatternUst>(serializer);
            }
            else if (objectType == typeof(PatternUst) || objectType.IsSubclassOf(typeof(PatternUst)))
            {
                var kind = (string)jObject[nameof(PatternUst.Kind)];
                ReflectionCache.TryGetClassType(kind, out Type type);
                var patternUst = (PatternUst)Activator.CreateInstance(type);
                target = patternUst;
                patternUst.Root = root;

                if (ancestors.Count > 0)
                {
                    patternUst.Parent = ancestors.Peek();
                }

                ancestors.Push(patternUst);

                if (patternUst is IRegexPattern regexPattern)
                {
                    if ((string)jObject[nameof(IRegexPattern.Regex)] is string regex)
                    {
                        regexPattern.RegexString = regex;
                    }

                    ReadTextSpan(jObject, patternUst, serializer);
                }
                else if (patternUst is PatternIntRangeLiteral patternIntRangeLiteral)
                {
                    if ((string)jObject[nameof(PatternIntLiteral.Value)] is string range)
                    {
                        patternIntRangeLiteral.ParseAndPopulate(range);
                    }

                    ReadTextSpan(jObject, patternUst, serializer);
                }
                else
                {
                    serializer.Populate(jObject.CreateReader(), target);
                }

                ancestors.Pop();
            }

            return target;
        }

        private static void ReadTextSpan(JObject jObject, PatternUst patternUst, JsonSerializer serializer)
        {
            if (jObject[nameof(PatternUst.TextSpan)] is JToken textSpanToken)
            {
                patternUst.TextSpan = textSpanToken.ToObject<TextSpan>(serializer);
            }
        }
    }
}
