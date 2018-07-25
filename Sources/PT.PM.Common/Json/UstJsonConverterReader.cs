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

            List<TextSpan> textSpans =
                jObject[nameof(Ust.TextSpan)]?.ToTextSpans(serializer).ToList();

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
            PropertyInfo[] properties = type.GetReadWriteClassProperties();
            foreach (var property in properties.Where(p => p.Name != nameof(RootUst.TextSpan)))
            {
                var propertyToken = token[property.Name];
                if (propertyToken != null)
                {
                    property.SetValue(ust, propertyToken.ToObject(property.PropertyType, serializer));
                }
            }

            return ust;
        }

        private Ust ReadAsRootUst(JsonSerializer serializer, JObject token, RootUst rootUst)
        {
            Ust[] nodes;
            var nodesToken = token[nameof(RootUst.Nodes)]?.ReadArray();
            if (nodesToken?.Length > 0)
            {
                nodes = new Ust[nodesToken.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = (Ust)ReadJson(nodesToken[i].CreateReader(), null, null, serializer);
                }
                rootUst.Nodes = nodes;
            }
            return rootUst;
        }

        private Ust ReadAsUsingDeclaration(JsonSerializer serializer, JObject token, UsingDeclaration usingUst)
        {
            string currentProperty = string.Empty;
            var nameNode = token[nameof(UsingDeclaration.Name)];
            if (nameNode != null)
            {
                usingUst.Name = (StringLiteral)ReadJson(nameNode.CreateReader(), typeof(StringLiteral), null, serializer);
            }
            return usingUst;
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
