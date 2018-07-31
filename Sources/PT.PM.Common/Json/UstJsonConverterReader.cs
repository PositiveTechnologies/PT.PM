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
        public Stack<RootUst> rootAncestors = new Stack<RootUst>();
        public Stack<Ust> ancestors = new Stack<Ust>();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanWrite => false;

        public CodeFile JsonFile { get; } = CodeFile.Empty;

        public bool IgnoreExtraProcess { get; set; } = false;

        public UstJsonNodesConverter NodesConverter { get; set; }

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
            NodesConverter = NodesConverter ?? new UstJsonNodesConverter(this, serializer);

            JObject jObject = JObject.Load(reader);
            return ReadJson(jObject, objectType, existingValue, serializer);
        }

        public Ust ReadJson(JObject jObject, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string kind = jObject[nameof(Ust.Kind)]?.ToString() ?? "";

            if (!ReflectionCache.TryGetClassType(kind, out Type type))
            {
                JsonUtils.LogError(Logger, JsonFile, jObject, $"Unknown UST {nameof(Ust.Kind)} {kind}");
                return null;
            }

            RootUst rootUst = null;
            Ust ust;

            if (type == typeof(RootUst))
            {
                string languageString = (string)jObject?[nameof(RootUst.Language)] ?? "";
                Language language = !string.IsNullOrEmpty(languageString)
                    ? languageString.ParseLanguages().FirstOrDefault()
                    : Uncertain.Language;
                CodeFile codeFile = (CodeFile)jObject[nameof(RootUst.SourceCodeFile)]?.ToObject(typeof(CodeFile), serializer);
                rootUst = new RootUst(codeFile, language);
                ProcessRootUst(rootUst);

                ust = rootUst;
            }
            else
            {
                ust = (Ust)Activator.CreateInstance(type);
            }



            try
            {
                ust = NodesConverter.Convert(jObject, ust);
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, jObject, ex);
            }

            return ust;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Use {GetType().Name.Replace("Reader", "Writer")} for JSON writing");
        }

        protected virtual void ProcessRootUst(RootUst rootUst)
        {
        }

        public virtual void ExtraProcess(Ust ust, JObject ustJObject, JsonSerializer serializer)
        {
        }
    }
}
