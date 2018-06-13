using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM.Common.Json
{
    public abstract class JsonBaseSerializer<T> : ILoggable
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeTextSpans { get; set; } = true;

        public bool Indented { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public bool IncludeCode { get; set; } = true;

        public bool LinearTextSpans { get; set; } = false;

        public bool NotStrict { get; set; } = false;

        public string EmptyTextSpanFormat { get; set; } = null;

        public CodeFile CurrectCodeFile { get; set; }

        public HashSet<CodeFile> CodeFiles { get; set; } = new HashSet<CodeFile>();

        public CodeFile JsonFile { get; protected set; } = CodeFile.Empty;

        public virtual T Deserialize(CodeFile jsonFile)
        {
            JsonFile = jsonFile;
            JsonSerializerSettings jsonSettings = PrepareSettings(false, jsonFile);
            return JsonConvert.DeserializeObject<T>(jsonFile.Code, jsonSettings);
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

        protected JsonSerializerSettings PrepareSettings(bool writer, CodeFile jsonFile)
        {
            JsonConverter jsonConverter;

            if (writer)
            {
                UstJsonConverterWriter jsonConverterWriter = CreateConverterWriter();
                jsonConverterWriter.IncludeTextSpans = IncludeTextSpans;
                jsonConverterWriter.ExcludeDefaults = ExcludeDefaults;
                jsonConverterWriter.Logger = Logger;

                jsonConverter = jsonConverterWriter;
            }
            else
            {
                UstJsonConverterReader jsonConverterReader = CreateConverterReader(jsonFile);

                jsonConverter = jsonConverterReader;
            }

            var textSpanJsonConverter = new TextSpanJsonConverter
            {
                EmptyTextSpanFormat = EmptyTextSpanFormat,
                IsLinear = LinearTextSpans,
                JsonFile = JsonFile,
                Logger = Logger,
                CodeFiles = CodeFiles,
                CurrentCodeFile = CurrectCodeFile
            };

            var jsonSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = SetMissingMemberHandling(),
                Formatting = Indented ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter>
                {
                    stringEnumConverter,
                    jsonConverter,
                    LanguageJsonConverter.Instance,
                    textSpanJsonConverter,
                    new CodeFileJsonConverter
                    {
                        TextSpanJsonConverter = textSpanJsonConverter,
                        ExcludeDefaults = ExcludeDefaults,
                        IncludeCode = IncludeCode,
                        JsonFile = JsonFile,
                        Logger = Logger
                    }
                }
            };

            return jsonSettings;
        }

        protected abstract UstJsonConverterReader CreateConverterReader(CodeFile jsonFile);

        protected abstract UstJsonConverterWriter CreateConverterWriter();

        protected virtual MissingMemberHandling SetMissingMemberHandling()
        {
            return NotStrict ? MissingMemberHandling.Ignore
                             : MissingMemberHandling.Error;
        }
    }
}
