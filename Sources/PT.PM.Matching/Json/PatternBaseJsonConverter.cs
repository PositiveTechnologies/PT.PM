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
    public class PatternJsonConverter : JsonConverterBase
    {
        public PatternJsonConverter(CodeFile jsonFile)
            : base(jsonFile)
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PatternUst) ||
                objectType.IsSubclassOf(typeof(PatternUst)) ||
                objectType == typeof(PatternRoot);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);

                object target = null;
                if (objectType == typeof(PatternRoot))
                {
                    var languagesArray = (JArray)jObject.GetValueIgnoreCase(nameof(PatternDto.Languages));
                    HashSet<Language> resultLanguages = languagesArray.Values<string>().ParseLanguages(patternLanguages: true);

                    target = new PatternRoot
                    {
                        Key = (string)jObject.GetValueIgnoreCase(nameof(PatternRoot.Key)) ?? "",
                        FilenameWildcard = (string)jObject.GetValueIgnoreCase(nameof(PatternRoot.FilenameWildcard)) ?? "",
                        Languages = resultLanguages,
                        DataFormat = (string)jObject.GetValueIgnoreCase(nameof(PatternRoot.DataFormat)) ?? "",
                        Node = jObject.GetValueIgnoreCase(nameof(PatternRoot.Node)).ToObject<PatternUst>(serializer)
                    };
                }
                else if (objectType == typeof(PatternUst) || objectType.IsSubclassOf(typeof(PatternUst)))
                {
                    var kind = (string)jObject.GetValueIgnoreCase(KindName);
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
