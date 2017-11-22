using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
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
            Ust target = CreateUst(jObject);

            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }
    }
}
