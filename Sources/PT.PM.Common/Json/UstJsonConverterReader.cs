using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Files;

namespace PT.PM.Common.Json
{
    public class UstJsonConverterReader : JsonConverter, ILoggable
    {
        private readonly Stack<RootUst> rootAncestors = new Stack<RootUst>();
        private readonly Stack<Ust> ancestors = new Stack<Ust>();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanWrite => false;

        public TextFile SerializedFile { get; } = TextFile.Empty;

        public bool IgnoreExtraProcess { get; set; } = false;

        public UstJsonConverterReader(TextFile serializedFile)
        {
            SerializedFile = serializedFile;
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
            string kind = jObject[nameof(Ust.Kind)]?.ToString() ?? "";

            if (!ReflectionCache.TryGetClassType(kind, out Type type))
            {
                JsonUtils.LogError(Logger, SerializedFile, jObject, $"Unknown UST {nameof(Ust.Kind)} {kind}");
                return null;
            }

            RootUst rootUst = null;
            Ust ust;

            if (type == typeof(RootUst))
            {
                string languageString = (string)jObject[nameof(RootUst.Language)] ?? "";
                Enum.TryParse(languageString, out Language language);
                rootUst = new RootUst(null, language);
                ProcessRootUst(rootUst);

                ust = rootUst;
            }
            else
            {
                ust = (Ust)Activator.CreateInstance(type);
            }

            if (rootAncestors.Count > 0)
            {
                ust.Root = rootAncestors.Peek();
            }
            if (ancestors.Count > 0 && ust is IUstWithParent ustWithParent)
            {
                ustWithParent.Parent = ancestors.Peek();
            }

            if (rootUst != null)
            {
                rootAncestors.Push(rootUst);
            }
            ancestors.Push(ust);

            try
            {
                serializer.Populate(jObject.CreateReader(), ust);
            }
            catch (Exception ex)
            {
                Logger.LogError(SerializedFile, jObject, ex);
            }

            JToken textSpanToken = jObject[nameof(Ust.TextSpan)];
            TextSpan[] textSpans;

            if (textSpanToken is JArray textSpanArray)
            {
                textSpans = new TextSpan[textSpanArray.Count];
                for (int i = 0; i < textSpanArray.Count; i++)
                {
                    textSpans[i] = textSpanArray[i].ToObject<TextSpan>(serializer);
                }
            }
            else if (textSpanToken is JToken token)
            {
                textSpans = new [] {token.ToObject<TextSpan>(serializer)};
            }
            else
            {
                textSpans = new TextSpan[0];
            }

            ust.TextSpans = textSpans;

            if (!IgnoreExtraProcess)
            {
                ExtraProcess(ust, jObject, serializer);
            }

            if (rootUst != null)
            {
                rootAncestors.Pop();
            }
            ancestors.Pop();

            return ust;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Use {GetType().Name.Replace("Reader", "Writer")} for JSON writing");
        }

        protected virtual void ProcessRootUst(RootUst rootUst)
        {
        }

        protected virtual void ExtraProcess(Ust ust, JObject ustJObject, JsonSerializer serializer)
        {
        }
    }
}
