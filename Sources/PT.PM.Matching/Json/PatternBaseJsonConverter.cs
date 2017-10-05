using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Common.Reflection;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Json
{
    public class PatternJsonConverter : JsonConverterBase
    {
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
                    var languagesArray = (JArray)jObject[nameof(PatternDto.Languages)];
                    HashSet<LanguageInfo> resultLanguages;
                    if (languagesArray?.Count > 0)
                    {
                        resultLanguages = new HashSet<LanguageInfo>();
                        foreach (string value in languagesArray.Values<string>())
                        {
                            LanguageInfo language = LanguageUtils.Languages.Values.FirstOrDefault(
                                    lang => string.Equals(lang.Key, value, StringComparison.InvariantCultureIgnoreCase));
                            if (language != null)
                            {
                                resultLanguages.Add(language);
                            }
                            else
                            {
                                Logger.LogError(new ConversionException("", message: $"Language \"{value}\" is not supported or wrong."));
                            }
                        }
                    }
                    else
                    {
                        resultLanguages = new HashSet<LanguageInfo>(LanguageUtils.PatternLanguages.Values);
                    }
                    target = new PatternRoot
                    {
                        Key = (string)jObject[nameof(PatternRoot.Key)] ?? "",
                        FilenameWildcard = (string)jObject[nameof(PatternRoot.FilenameWildcard)] ?? "",
                        Languages = resultLanguages,
                        DataFormat = (string)jObject[nameof(PatternRoot.DataFormat)] ?? "",
                        Node = jObject[nameof(PatternRoot.Node)].ToObject<PatternUst>(serializer)
                    };
                }
                else if (objectType == typeof(PatternUst) || objectType.IsSubclassOf(typeof(PatternUst)))
                {
                    var kind = (string)jObject[KindName];
                    var type = ReflectionCache.UstKindFullClassName.Value[kind];
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
