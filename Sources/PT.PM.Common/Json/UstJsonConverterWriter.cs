using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PT.PM.Common.Utils;

namespace PT.PM.Common.Json
{
    public class UstJsonConverterWriter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeTextSpans { get; set; } = false;

        public bool IsLineColumn { get; set; }

        public bool ExcludeDefaults { get; set; } = true;

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Use {GetType().Name.Replace("Writer", "Reader")} for JSON reading");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = GetJObject(value, serializer);
            jObject.WriteTo(writer);
        }

        protected JObject GetJObject(object value, JsonSerializer serializer)
        {
            var jObject = new JObject();
            Type type = value.GetType();
            jObject.Add(nameof(Ust.Kind), type.Name);
            PropertyInfo[] properties = type.GetSerializableProperties(out _);

            if (type == typeof(RootUst))
            {
                // Back compatibility with external serializers
                jObject.Add("SourceCodeFile", JToken.FromObject(((RootUst)value).SourceFile, serializer));
            }

            foreach (PropertyInfo prop in properties)
            {
                string propName = prop.Name;
                bool include = propName != nameof(ILoggable.Logger);
                if (include)
                {
                    if (ExcludeDefaults)
                    {
                        Type propType = prop.PropertyType;
                        object propValue = prop.GetValue(value);
                        if (propValue == null)
                        {
                            include = false;
                        }
                        else if (propType == typeof(string))
                        {
                            include = !string.IsNullOrEmpty((string) propValue);
                        }
                        else if (propType.IsArray)
                        {
                            include = ((Array) propValue).Length > 0;
                        }
                        else
                        {
                            include = !propValue.Equals(propType.GetDefaultValue());
                        }
                    }

                    if (include)
                    {
                        include = propName != nameof(Ust.TextSpan) || IncludeTextSpans;
                    }
                }

                if (type.Name == nameof(RootUst))
                {
                    if (include && propName == nameof(RootUst.Node) || propName == nameof(RootUst.Sublanguages))
                    {
                        include = false;
                    }
                }

                if (include)
                {
                    JToken jToken = null;

                    object propVal = prop.GetValue(value, null);
                    if (propVal is IEnumerable<Language> languages)
                    {
                        jToken = JToken.FromObject(string.Join(",", languages), serializer);
                    }
                    else if (propVal != null)
                    {
                        if (propVal is TextSpan[] textSpans)
                        {
                            jToken = textSpans.Length == 1
                                ? JToken.FromObject(textSpans[0], serializer)
                                : JArray.FromObject(textSpans, serializer);
                        }
                        else
                        {
                            jToken = JToken.FromObject(propVal, serializer);
                        }
                    }

                    if (jToken != null)
                    {
                        var jsonPropertyAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                        if (jsonPropertyAttr?.PropertyName != null)
                        {
                            propName = jsonPropertyAttr.PropertyName;
                        }

                        jObject.Add(propName, jToken);
                    }
                }
            }

            return jObject;
        }

        protected static bool WriteCollection<T>(JObject writeTo, string propertyName, IEnumerable<T> values, JsonSerializer jsonSerializer)
        {
            if (values == null)
                return false;

            List<T> valuesList = values.ToList();

            JToken serializedObject = valuesList.Count == 0
                ? null
                : valuesList.Count == 1
                ? JToken.FromObject(valuesList[0], jsonSerializer)
                : JArray.FromObject(valuesList, jsonSerializer);

            if (serializedObject == null)
                return false;

            writeTo.Add(propertyName, serializedObject);

            return true;
        }
    }
}
