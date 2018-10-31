using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            throw new InvalidOperationException($"Use {(GetType().Name.Replace("Writer", "Reader"))} for JSON reading");
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
            PropertyInfo[] properties = type.GetReadWriteClassProperties();

            if (type.Name == nameof(RootUst))
            {
                jObject.Add(nameof(RootUst.Language), ((RootUst)value).Language.Key);
            }

            foreach (PropertyInfo prop in properties)
            {
                string propName = prop.Name;

                bool include = prop.CanWrite;
                if (include)
                {
                    include =
                        propName != "Root" &&
                        propName != "Parent" &&
                        propName != nameof(ILoggable.Logger) &&
                        propName != nameof(Ust.InitialTextSpans);
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
                                include = !string.IsNullOrEmpty(((string)propValue));
                            }
                            else if (propType.IsArray)
                            {
                                include = ((Array)propValue).Length > 0;
                            }
                            else
                            {
                                include = !propValue.Equals(GetDefaultValue(propType));
                            }
                        }
                        if (include)
                        {
                            include = propName != nameof(Ust.TextSpan) || IncludeTextSpans;
                        }
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

                    if (value is Ust ust && propName == nameof(Ust.TextSpan) &&
                        ust.InitialTextSpans != null && ust.InitialTextSpans.Count > 0)
                    {
                        jToken = JToken.FromObject(ust.InitialTextSpans, serializer);
                    }
                    else
                    {
                        object propVal = prop.GetValue(value, null);
                        if (propVal is IEnumerable<Language> languages)
                        {
                            jToken = JToken.FromObject(string.Join(",", languages.Select(l => l.Key)), serializer);
                        }
                        else if (propVal != null)
                        {
                            jToken = JToken.FromObject(propVal, serializer);
                        }
                    }

                    if (jToken != null)
                    {
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

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
