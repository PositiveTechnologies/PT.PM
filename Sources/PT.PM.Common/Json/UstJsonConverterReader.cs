﻿using Newtonsoft.Json;
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

        public CodeFile JsonFile { get; } = CodeFile.Empty;

        public bool IgnoreExtraProcess { get; set; } = false;

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
                Logger.LogError(JsonFile, jObject, ex);
            }

            JToken textSpanToken = jObject[nameof(Ust.TextSpan)];
            TextSpan textSpan = default;

            if (textSpanToken is JArray textSpanArray)
            {
                if (textSpanArray.Count > 1)
                {
                    var textSpans = new List<TextSpan>(textSpanArray.Count);
                    for (int i = 0; i < textSpanArray.Count; i++)
                    {
                        TextSpan arrayTextSpan = textSpanArray[i].ToObject<TextSpan>(serializer);

                        if (i == 0)
                        {
                            textSpan = arrayTextSpan;
                        }

                        textSpans.Add(arrayTextSpan);
                    }

                    if (rootAncestors.Count > 0)
                    {
                        rootAncestors.Peek().TextSpans.Add(ust, textSpans);
                    }
                }
                else
                {
                    textSpan = textSpanArray[0].ToObject<TextSpan>(serializer);
                }
            }
            else if (textSpanToken is JToken token)
            {
                textSpan = token.ToObject<TextSpan>(serializer);
            }

            ust.TextSpan = textSpan;

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
