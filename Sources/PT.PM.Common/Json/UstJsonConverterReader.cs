using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.Common.Json
{
    public class UstJsonConverterReader : JsonConverter, ILoggable
    {
        private Stack<RootUst> rootAncestors = new Stack<RootUst>();
        private Stack<Ust> ancestors = new Stack<Ust>();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanWrite => false;

        public CodeFile JsonFile { get; } = CodeFile.Empty;

        public bool IgnoreExtraProcess { get; set; } = false;

        public LiteralJsonReader LiteralJsonReader { get; } = new LiteralJsonReader();

        public TokenJsonReader TokenJsonReader { get; } = new TokenJsonReader();

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

            return ReadJson(jObject, objectType, existingValue, serializer);
        }

        private Ust ReadJson(JObject jObject, Type objectType, object existingValue, JsonSerializer serializer)
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
                var codeFile = jObject[nameof(RootUst.SourceCodeFile)]?.ToObject<CodeFile>(serializer);
                rootUst = new RootUst(codeFile, language);
                ProcessRootUst(rootUst);

                ust = rootUst;
            }
            else
            {
                var constructor = type.GetConstructor(new Type[0]);
                ust = (Ust)constructor.Invoke(null);
            }

            if (rootAncestors.Count > 0)
            {
                ust.Root = rootAncestors.Peek();
            }

            if (ancestors.Count > 0)
            {
                ust.Parent = ancestors.Peek();
            }

            if (rootUst != null)
            {
                rootAncestors.Push(rootUst);
            }
            ancestors.Push(ust);

            FillTextSpans(jObject, ust, serializer);
            {
                serializer.Populate(jObject.CreateReader(), ust);
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, jObject, ex);
            }


            try
            {
                ust = ReadAsObject(serializer, jObject, ust, type);
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, jObject, ex);
            }

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

        private Ust ReadAsObject(JsonSerializer serializer, JObject token, Ust ust, Type type)
        {
            if (type.IsSubclassOf(typeof(Literal)))
            {
                ust = LiteralJsonReader.Read(token, ust);
            }
            else if (type.IsSubclassOf(typeof(Token)))
            {
                ust = TokenJsonReader.Read(token, ust);
            }
            else
            {
                PropertyInfo[] properties = type.GetReadWriteClassProperties();
                foreach (var property in properties.Where(p => p.Name != nameof(RootUst.TextSpan)))
                {
                    var propertyToken = token[property.Name];
                    if (propertyToken == null)
                    {
                        continue;
                    }

                    if (propertyToken.Type != JTokenType.Array && propertyToken[nameof(Ust.Kind)] != null)
                    {
                        property.SetValue(ust, ReadJson((JObject)propertyToken, null, null, serializer));
                    }
                    else
                    {
                        property.SetValue(ust, propertyToken.ToObject(property.PropertyType, serializer));
                    }
                }
            }
            return ust;
        }

        private void FillTextSpans(JObject token, Ust ust, JsonSerializer serializer)
        {
            List<TextSpan> textSpans = token[nameof(Ust.TextSpan)]?.ToTextSpans(serializer).ToList();

            if (textSpans?.Count > 0)
            {
                if (textSpans.Count == 1)
                {
                    ust.TextSpan = textSpans[0];
                }
                else
                {
                    ust.InitialTextSpans = textSpans;
                    ust.TextSpan = textSpans[0];
                }
            }
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
