using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Reflection;

namespace PT.PM.Common.Json
{
    public abstract class JsonConverterBase : JsonConverter
    {
        protected const string KindName = "Kind";

        public bool IncludeTextSpans { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = GetJObject(value, serializer);
            jObject.WriteTo(writer);
        }

        protected JObject GetJObject(object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            Type type = value.GetType();
            jObject.Add(KindName, type.Name);
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
                            else if (propType == typeof(string))
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

                if (type.Name == nameof(RootUst))
                {
                    if (include && propName == nameof(RootUst.Node) || propName == nameof(RootUst.Sublanguages))
                    {
                        include = false;
                    }
                }

                if (include)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        JToken jToken = JToken.FromObject(propVal, serializer);
                        jObject.Add(propName, jToken);
                    }
                }
            }

            return jObject;
        }

        protected Ust CreateUst(object jObjectOrToken)
        {
            JObject jObject = jObjectOrToken as JObject;
            JToken jToken = jObject == null ? jObjectOrToken as JToken : null;

            string ustKind = jObject != null
                ? ((string)jObject[KindName]) : jToken != null
                ? ((string)jToken[KindName]) : "";

            Type type = ReflectionCache.UstKindFullClassName.Value[ustKind];

            Ust ust;
            if (type == typeof(RootUst))
            {
                string languageString = jObject != null
                    ? ((string)jObject[nameof(RootUst.Language)])
                    : jToken != null
                    ? ((string)jToken[nameof(RootUst.Language)])
                    : "";
                Language language = Uncertain.Language;
                if (!string.IsNullOrEmpty(languageString))
                { 
                    LanguageUtils.Languages.TryGetValue(languageString, out language);
                }
                ust = (Ust)Activator.CreateInstance(type, null, language);
            }
            else
            {
                ust = (Ust)Activator.CreateInstance(type);
            }

            return ust;
        }

        private object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }
    }
}
