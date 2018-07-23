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
                ust = (Ust)Activator.CreateInstance(type);
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
                ust = ReadAsObject(serializer, jObject.CreateReader(), ust, type);
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

        private Ust ReadAsObject(JsonSerializer serializer, JsonReader reader, Ust ust, Type type)
        {
            if (ust is RootUst rootUst)
            {
                ust = ReadAsRootUst(serializer, reader, rootUst);
            }
            else if (ust is UsingDeclaration usingUst)
            {
                ust = ReadAsUsingDeclaration(reader, usingUst, serializer);
            }
            else if (type.IsSubclassOf(typeof(Literal)))
            {
                ust = LiteralJsonReader.Read(reader, ust);
            }
            else if (type.IsSubclassOf(typeof(Token)))
            {
                ust = TokenJsonReader.Read(reader, ust);
            }
            else
            {
                serializer.Populate(reader, ust);
            }
            return ust;
        }

        private Ust ReadAsRootUst(JsonSerializer serializer, JsonReader reader, RootUst rootUst)
        {
            List<Ust> nodes = new List<Ust>();
            string currentProperty = string.Empty;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    currentProperty = reader.Value.ToString();
                    reader.Read();
                }
                if (currentProperty == nameof(RootUst.Nodes))
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            nodes.Add((Ust)ReadJson(reader, null, null, serializer));
                        }
                    }
                    rootUst.Nodes = nodes.ToArray();
                    break;
                }
            }
            return rootUst;
        }

        private Ust ReadAsUsingDeclaration(JsonReader reader, UsingDeclaration usingUst, JsonSerializer serializer)
        {
            string currentProperty = string.Empty;
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currentProperty = reader.Value.ToString();
                        reader.Read();
                    }
                    if (currentProperty == nameof(UsingDeclaration.Name))
                    {
                        usingUst.Name = (StringLiteral)ReadJson(reader, null, null, serializer);
                        break;
                    }
                }
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
