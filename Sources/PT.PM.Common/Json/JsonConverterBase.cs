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

        public const string KindName = "Kind";

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
                    JToken jToken = null;

                    if (value is Ust ust && propName == nameof(Ust.TextSpan) &&
                        ust.InitialTextSpans != null && ust.InitialTextSpans.Count > 0)
                    {
                        jToken = JToken.FromObject(ust.InitialTextSpans, serializer);
                    }
                    else
                    {
                        object propVal = prop.GetValue(value, null);
                        if (propVal != null)
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

        protected Ust CreateOrGetUst(object jObjectOrToken, Type type = null)
        {
            JObject jObject = jObjectOrToken as JObject;
            JToken jToken = jObject == null ? jObjectOrToken as JToken : null;

            if (type == null)
            {
                string ustKind = jObject != null
                    ? (string)jObject[KindName]
                    : jToken != null
                    ? (string)jToken[KindName]
                    : "";

                if (string.IsNullOrEmpty(ustKind) ||
                    !ReflectionCache.TryGetClassType(ustKind, out type))
                {
                    string errorMessage = $"{KindName} field " +
                        (ustKind == null ? "undefined" : $"incorrect ({ustKind})");
                    Logger.LogError(JsonFile, jObjectOrToken as IJsonLineInfo, errorMessage);
                    return null;
                }
            }

            JToken textSpanTokenWrapper = jObject != null
                ? jObject[nameof(Ust.TextSpan)]
                : jToken?[nameof(Ust.TextSpan)];

            List<TextSpan> textSpans = null;

            if (textSpanTokenWrapper != null)
            {
                textSpans = GetCommonTextSpan(textSpanTokenWrapper).ToList();
                TextSpan commonTextSpan = textSpans.FirstOrDefault();

                if (!commonTextSpan.IsZero && existingUsts.TryGetValue(commonTextSpan, out List<Ust> usts))
                {
                    Ust sameTypeUst = usts.FirstOrDefault(u => u.GetType() == type);
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
                    ? (string)jObject[nameof(RootUst.Language)]
                    : jToken != null
                    ? (string)jToken[nameof(RootUst.Language)]
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

            if (textSpans != null && textSpans.Count > 0)
            {
                if (textSpans.Count == 1)
                {
                    ust.TextSpan = textSpans[0];
                }
                else
                {
                    ust.InitialTextSpans = textSpans;
                    ust.TextSpan = textSpans.Union();
                }
                existingUsts.Add(ust.TextSpan, ust);
            }

            return ust;
        }

        protected IEnumerable<TextSpan> GetCommonTextSpan(JToken textSpanToken)
        {
            return textSpanToken
                .GetTokenOrTokensArray()
                .Select(token => token.ToObject<TextSpan>(jsonSerializer));
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
