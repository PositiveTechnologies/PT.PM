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
        private MultiMap<TextSpan, Ust> existingUsts = new MultiMap<TextSpan, Ust>();

        protected const string KindName = "Kind";

        protected JsonSerializer jsonSerializer;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CodeFile JsonFile { get; } = CodeFile.Empty;

        public bool IncludeTextSpans { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public JsonConverterBase(CodeFile jsonFile)
        {
            JsonFile = jsonFile;
        }

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

        protected Ust CreateOrGetUst(object jObjectOrToken)
        {
            JObject jObject = jObjectOrToken as JObject;
            JToken jToken = jObject == null ? jObjectOrToken as JToken : null;

            string ustKind = jObject != null
                ? (string)jObject.GetValueIgnoreCase(KindName)
                : jToken != null
                ? (string)jToken.GetValueIgnoreCase(KindName)
                : "";

            Type type;
            if (string.IsNullOrEmpty(ustKind) ||
                !ReflectionCache.TryGetClassType(ustKind, out type))
            {
                string errorMessage = $"{KindName} field " +
                    (ustKind == null ? "undefined" : $"incorrect ({ustKind})");
                Logger.LogError(JsonFile, jObjectOrToken as IJsonLineInfo, errorMessage);
                return null;
            }

            JToken textSpanToken = jObject != null
                ? jObject.GetValueIgnoreCase(nameof(Ust.TextSpan))
                : jToken != null
                ? jToken.GetValueIgnoreCase(nameof(Ust.TextSpan))
                : null;

            TextSpan textSpan = TextSpan.Empty;
            if (textSpanToken != null)
            {
                textSpan = textSpanToken.ToObject<TextSpan>(jsonSerializer);
                if (!textSpan.IsEmpty && existingUsts.TryGetValue(textSpan, out List<Ust> usts))
                {
                    var sameTypeUst = usts.FirstOrDefault(u => u.GetType() == type);
                    if (sameTypeUst != null)
                    {
                        return sameTypeUst;
                    }
                }
            }

            Ust ust;
            if (type == typeof(RootUst))
            {
                string languageString = jObject != null
                    ? (string)jObject.GetValueIgnoreCase(nameof(RootUst.Language))
                    : jToken != null
                    ? (string)jToken.GetValueIgnoreCase(nameof(RootUst.Language))
                    : "";
                Language language = Uncertain.Language;
                if (!string.IsNullOrEmpty(languageString))
                {
                    language = languageString.ParseLanguages().FirstOrDefault();
                }
                ust = (Ust)Activator.CreateInstance(type, null, language);
            }
            else
            {
                ust = (Ust)Activator.CreateInstance(type);
            }

            if (!textSpan.IsEmpty)
            {
                ust.TextSpan = textSpan;
                existingUsts.Add(textSpan, ust);
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
