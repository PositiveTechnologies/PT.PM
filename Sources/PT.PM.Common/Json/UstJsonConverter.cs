using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;

namespace PT.PM.Common.Json
{
    public class UstJsonConverter : JsonConverterBase
    {
        public UstJsonConverter(CodeFile jsonFile)
            : base(jsonFile)
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            jsonSerializer = serializer;

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jObject = JObject.Load(reader);
            string kind = jObject[KindName].ToString();

            if (ReflectionCache.TryGetClassType(kind, out Type type))
            {
                Ust target = CreateUst(jObject, type);
                if (target == null)
                {
                    return null;
                }

                JsonReader newReader = jObject.CreateReader();
                serializer.Populate(newReader, target);

                return target;
            }
            else
            {
                JsonUtils.LogError(Logger, JsonFile, jObject, $"Unknown UST {nameof(Ust.Kind)} {kind}");
            }

            return null;
        }
    }
}
