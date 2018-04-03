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

            Ust target;
            JsonReader newReader = null;
            if (ReflectionCache.TryGetClassType(kind, out Type type))
            {
                target = CreateOrGetUst(jObject, type);
                if (target == null)
                {
                    return null;
                }
                newReader = jObject.CreateReader();
            }
            else
            {
                // Try load from Ust subfield.
                JToken jToken = jObject[nameof(Ust)];
                target = CreateOrGetUst(jToken);
                if (target == null)
                {
                    return null;
                }
                newReader = jToken.CreateReader();
            }

            serializer.Populate(newReader, target);

            return target;
        }
    }
}
