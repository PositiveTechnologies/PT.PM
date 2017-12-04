using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;

namespace PT.PM.Common.Json
{
    public class UstJsonConverter : JsonConverterBase
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jObject = JObject.Load(reader);
            string kind = jObject[KindName].ToString();

            Ust target;
            JsonReader newReader;
            if (!ReflectionCache.UstKindFullClassName.Value.ContainsKey(kind))
            {
                // Try load from Ust subfield.
                JToken jToken = jObject[nameof(Ust)];
                target = CreateUst(jToken);
                newReader = jToken.CreateReader();
            }
            else
            {
                target = CreateUst(jObject);
                newReader = jObject.CreateReader();
            }
            serializer.Populate(newReader, target);
            return target;
        }
    }
}
