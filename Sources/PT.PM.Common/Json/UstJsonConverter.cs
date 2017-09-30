using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Reflection;

namespace PT.PM.Common
{
    public class UstJsonConverter : JsonConverter
    {
        private const string PropertyName = nameof(Ust.Kind);

        public bool IncludeTextSpans { get; set; } = false;

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
                    var kind = jObject[PropertyName].ToString();
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            Type type = value.GetType();
            jObject.Add(PropertyName, type.Name);
            PropertyInfo[] properties = type.GetClassProperties();
            foreach (PropertyInfo prop in properties)
            {
                bool ignore = prop.Name == nameof(Ust.TextSpan) &&
                    (((TextSpan)prop.GetValue(value)).IsEmpty || !IncludeTextSpans);
                if (prop.CanWrite &&
                    !ignore &&
                    prop.Name != nameof(Ust.Root) && prop.Name != nameof(Ust.Parent))
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        jObject.Add(prop.Name, JToken.FromObject(propVal, serializer));
                    }
                }
            }
            jObject.WriteTo(writer);
        }
    }
}
