using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace PT.PM.Common.Json
{
    public abstract class JsonConverterBase : JsonConverter, ILoggable
    {
        protected const string KindName = "Kind";

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
                LogError(jObjectOrToken as IJsonLineInfo, errorMessage);
                return null;
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

            return ust;
        }

        protected void LogError(IJsonLineInfo jsonLineInfo, Exception ex, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonLineInfo, out TextSpan errorTextSpan);
            errorMessage += "; " + ex.FormatExceptionMessage();
            LogErrorOrWarning(isError, errorMessage, errorTextSpan);
        }

        protected void LogError(IJsonLineInfo jsonLineInfo, string message, bool isError = true)
        {
            string errorMessage = GenerateErrorPositionMessage(jsonLineInfo, out TextSpan errorTextSpan);
            errorMessage += "; " + message;
            LogErrorOrWarning(isError, errorMessage, errorTextSpan);
        }

        protected string GenerateErrorPositionMessage(IJsonLineInfo jsonLineInfo, out TextSpan errorTextSpan)
        {
            int errorLine = CodeFile.StartLine;
            int errorColumn = CodeFile.StartColumn;
            if (jsonLineInfo != null)
            {
                errorLine = jsonLineInfo.LineNumber;
                errorColumn = jsonLineInfo.LinePosition;
            }
            errorTextSpan = new TextSpan(
                JsonFile.GetLinearFromLineColumn(errorLine, errorColumn), 0);
            LineColumnTextSpan lcTextSpan = new LineColumnTextSpan(errorLine, errorColumn);
            return $"File position: {lcTextSpan}";
        }

        private void LogErrorOrWarning(bool isError, string errorMessage, TextSpan errorTextSpan)
        {
            var exception = new ConversionException(JsonFile, null, errorMessage) { TextSpan = errorTextSpan };
            if (isError)
            {
                Logger.LogError(exception);
            }
            else
            {
                Logger.LogInfo($"{JsonFile}: " + errorMessage);
            }
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
