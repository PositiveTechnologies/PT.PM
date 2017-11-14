using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Linq;

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

                var kind = jObject[KindName].ToString();
                Type type = ReflectionCache.UstKindFullClassName.Value[kind];

                Ust target;
                if (type == typeof(RootUst))
                {
                    Language language = ((string)jObject[nameof(RootUst.Language)]).ParseLangs().FirstOrDefault();
                    target = (Ust)Activator.CreateInstance(type, null, language);
                }
                else
                {
                    target = (Ust)Activator.CreateInstance(type);
                }

                serializer.Populate(jObject.CreateReader(), target);

                JToken textSpanObj = jObject[nameof(Ust.TextSpan)];
                if (textSpanObj != null)
                {
                    int start = textSpanObj[nameof(TextSpan.Start)]?.ToObject<int>() ?? 0;
                    int length = textSpanObj[nameof(TextSpan.Length)]?.ToObject<int>() ?? 0;
                    target.TextSpan = new TextSpan(start, length);
                }

                return target;
            }

            return null;
        }
    }
}
