using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Json;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.Json
{
    public class PatternJsonConverterReader : UstJsonConverterReader
    {
        public PatternJsonConverterReader(CodeFile jsonFile)
            : base(jsonFile)
        {
        }

        public override bool CanConvert(Type objectType) => objectType.CanConvert();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);

                object target = null;
                if (objectType == typeof(PatternRoot))
                {
                    var languagesArray = (JArray)jObject[nameof(PatternDto.Languages)];
                    HashSet<Language> resultLanguages = languagesArray.Values<string>().ParseLanguages(patternLanguages: true);

                    target = new PatternRoot
                    {
                        Key = (string)jObject[nameof(PatternRoot.Key)] ?? "",
                        FilenameWildcard = (string)jObject[nameof(PatternRoot.FilenameWildcard)] ?? "",
                        Languages = resultLanguages,
                        DataFormat = (string)jObject[nameof(PatternRoot.DataFormat)] ?? "",
                        CodeFile = jObject[nameof(PatternRoot.CodeFile)].ToObject<CodeFile>(serializer),
                        Node = jObject[nameof(PatternRoot.Node)].ToObject<PatternUst>(serializer)
                    };
                }
                else if (objectType == typeof(PatternUst) || objectType.IsSubclassOf(typeof(PatternUst)))
                {
                    var kind = (string)jObject[UstJsonKeys.KindName];
                    ReflectionCache.TryGetClassType(kind, out Type type);
                    target = Activator.CreateInstance(type);
                    serializer.Populate(jObject.CreateReader(), target);
                }
                else
                {
                    throw new FormatException("Invalid JSON");
                }

                return target;
            }

            return null;
        }
    }
}
