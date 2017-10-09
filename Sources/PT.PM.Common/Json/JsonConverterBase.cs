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
    public abstract class JsonConverterBase : JsonConverter, ILoggable
    {
        protected const string KindName = "Kind";

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeTextSpans { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            Type type = value.GetType();
            jObject.Add(KindName, type.Name);
            PropertyInfo[] properties = type.GetClassProperties();
            foreach (PropertyInfo prop in properties)
            {
                string propName = prop.Name;
                bool include = prop.CanWrite;
                if (include)
                {
                    include =
                        propName != nameof(Ust.Root) &&
                        propName != nameof(Ust.Parent) &&
                        propName != nameof(ILoggable.Logger);
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
                            else if(propType == typeof(string))
                            {
                                include = !string.IsNullOrEmpty(((string)propValue));
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
                if (include)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        object serializeObj = propVal is IEnumerable<Language> languages
                            ? languages.Select(lang => lang.Key)
                            : propVal;
                        jObject.Add(propName, JToken.FromObject(serializeObj, serializer));
                    }
                }
            }
            jObject.WriteTo(writer);
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}
