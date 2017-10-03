using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Json;
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
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);

                object target = null;
                if (objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust)))
                {
                    var kind = jObject[nameof(Ust)].ToString();
                    var type = ReflectionCache.UstKindFullClassName.Value[kind];
                    target = Activator.CreateInstance(type);
                }
                else
                {
                    throw new FormatException("Invalid JSON");
                }

                serializer.Populate(jObject.CreateReader(), target);
                return target;
            }

            return null;
        }
    }
}
