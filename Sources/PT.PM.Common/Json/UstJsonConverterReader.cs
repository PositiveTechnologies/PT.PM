using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Json
{
    public class UstJsonConverterReader : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanWrite => false;

        public CodeFile JsonFile { get; } = CodeFile.Empty;

        public UstJsonConverterReader(CodeFile jsonFile)
        {
            JsonFile = jsonFile;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jObject = JObject.Load(reader);
            string kind = jObject[UstJsonKeys.KindName].ToString();

            if (ReflectionCache.TryGetClassType(kind, out Type type))
            {
                Ust target = CreateUst(jObject, serializer, type);
                if (target == null)
                {
                    return null;
                }

                JsonReader newReader = jObject.CreateReader();
                serializer.Populate(newReader, target);

                return target;
            }
            else
            {
                JsonUtils.LogError(Logger, JsonFile, jObject, $"Unknown UST {nameof(Ust.Kind)} {kind}");
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Use {(GetType().Name.Replace("Reader", "Writer"))} for JSON writing");
        }

        protected Ust CreateUst(object jObjectOrToken, JsonSerializer serializer, Type type = null)
        {
            JObject jObject = jObjectOrToken as JObject;
            JToken jToken = jObject == null ? jObjectOrToken as JToken : null;

            if (type == null)
            {
                string ustKind = jObject != null
                    ? (string)jObject[UstJsonKeys.KindName]
                    : jToken != null
                    ? (string)jToken[UstJsonKeys.KindName]
                    : "";

                if (string.IsNullOrEmpty(ustKind) ||
                    !ReflectionCache.TryGetClassType(ustKind, out type))
                {
                    string errorMessage = $"{UstJsonKeys.KindName} field " +
                        (ustKind == null ? "undefined" : $"incorrect ({ustKind})");
                    Logger.LogError(JsonFile, jObjectOrToken as IJsonLineInfo, errorMessage);
                    return null;
                }
            }

            JToken textSpanTokenWrapper = jObject != null
                ? jObject[nameof(Ust.TextSpan)]
                : jToken?[nameof(Ust.TextSpan)];

            List<TextSpan> textSpans =
                textSpanTokenWrapper?.ToTextSpans(serializer).ToList() ?? null;

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
                    ust.TextSpan = textSpans.First();
                }
            }

            return ust;
        }
    }
}
