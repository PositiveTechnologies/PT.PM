using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using PT.PM.Common.Files;

namespace PT.PM.Common.Json
{
    public abstract class JsonBaseSerializer<T> : ISerializer
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeTextSpans { get; set; } = true;

        public bool Indented { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public bool IncludeCode { get; set; } = true;

        public bool LineColumnTextSpans { get; set; } = false;

        public bool Strict { get; set; } = false;

        public string EmptyTextSpanFormat { get; set; } = null;

        public TextFile CurrectSourceFile { get; set; }

        public HashSet<IFile> SourceFiles { get; set; } = new HashSet<IFile>();

        public Action<(IFile, TimeSpan)> ReadSourceFileAction { get; set; }

        public TextFile SerializedFile { get; protected set; } = TextFile.Empty;

        public bool IgnoreExtraProcess { get; set; } = false;

        public virtual T Deserialize(TextFile serializedFile)
        {
            SerializedFile = serializedFile;
            JsonSerializerSettings jsonSettings = PrepareSettings(false, serializedFile);
            return JsonConvert.DeserializeObject<T>(serializedFile.Data, jsonSettings);
        }

        public virtual string Serialize(T node)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings(true, null);
            return JsonConvert.SerializeObject(node, jsonSettings);
        }

        public string Serialize(IEnumerable<T> nodes)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings(true, null);
            return JsonConvert.SerializeObject(nodes, jsonSettings);
        }

        protected JsonSerializerSettings PrepareSettings(bool writer, TextFile serializedFile)
        {
            JsonConverter jsonConverter;

            if (writer)
            {
                UstJsonConverterWriter jsonConverterWriter = CreateConverterWriter();
                jsonConverterWriter.IncludeTextSpans = IncludeTextSpans;
                jsonConverterWriter.ExcludeDefaults = ExcludeDefaults;
                jsonConverterWriter.Logger = Logger;
                jsonConverterWriter.IsLineColumn = LineColumnTextSpans;

                jsonConverter = jsonConverterWriter;
            }
            else
            {
                UstJsonConverterReader jsonConverterReader = CreateConverterReader(serializedFile);
                jsonConverterReader.Logger = Logger;
                jsonConverterReader.IgnoreExtraProcess = IgnoreExtraProcess;

                jsonConverter = jsonConverterReader;
            }

            var textSpanJsonConverter = new TextSpanJsonConverter
            {
                EmptyTextSpanFormat = EmptyTextSpanFormat,
                IsLineColumn = LineColumnTextSpans,
                SerializedFile = SerializedFile,
                Logger = Logger,
                SourceFiles = SourceFiles,
                CurrentSourceFile = CurrectSourceFile
            };

            var sourceFileJsonConverter = new SourceFileJsonConverter
            {
                ExcludeDefaults = ExcludeDefaults,
                IncludeCode = IncludeCode,
                SerializedFile = SerializedFile,
                Logger = Logger
            };

            sourceFileJsonConverter.ReadSourceFileAction = ReadSourceFileAction;

            sourceFileJsonConverter.SetCurrentSourceFileAction = sourceFile =>
            {
                textSpanJsonConverter.CurrentSourceFile = sourceFile;
            };

            var jsonSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = SetMissingMemberHandling(),
                Formatting = Indented ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter>
                {
                    stringEnumConverter,
                    jsonConverter,
                    textSpanJsonConverter,
                    sourceFileJsonConverter
                }
            };

            return jsonSettings;
        }

        protected abstract UstJsonConverterReader CreateConverterReader(TextFile serializedFile);

        protected abstract UstJsonConverterWriter CreateConverterWriter();

        protected virtual MissingMemberHandling SetMissingMemberHandling()
        {
            return Strict ? MissingMemberHandling.Error : MissingMemberHandling.Ignore;
        }
    }
}
