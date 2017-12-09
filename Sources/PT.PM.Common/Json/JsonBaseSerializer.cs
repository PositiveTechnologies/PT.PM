using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM.Common.Json
{
    public abstract class JsonBaseSerializer<T> : ILoggable
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SourceCodeFile SourceCodeFile { get; set; }

        public bool IncludeTextSpans { get; set; } = true;

        public bool Indented { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public bool IncludeCode { get; set; } = false;

        public bool ShortTextSpans { get; set; } = true;

        public string EmptyTextSpanFormat { get; set; } = null;

        protected abstract JsonConverterBase CreateConverterBase();

        public virtual T Deserialize(string data)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.DeserializeObject<T>(data, jsonSettings);
        }

        public virtual string Serialize(T node)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(node, jsonSettings);
        }

        public string Serialize(IEnumerable<T> nodes)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(nodes, jsonSettings);
        }

        public JsonSerializerSettings PrepareSettings()
        {
            JsonConverterBase jsonConverterBase = CreateConverterBase();
            jsonConverterBase.IncludeTextSpans = IncludeTextSpans;
            jsonConverterBase.ExcludeDefaults = ExcludeDefaults;
            jsonConverterBase.Logger = Logger;
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Indented ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter>
                {
                    stringEnumConverter,
                    jsonConverterBase,
                    new LanguageJsonConverter(),
                    new TextSpanJsonConverter
                    {
                        ShortFormat = ShortTextSpans,
                        EmptyTextSpanFormat = EmptyTextSpanFormat,
                    },
                    new SourceCodeFileJsonConverter
                    {
                        ExcludeDefaults = ExcludeDefaults,
                        IncludeCode = IncludeCode
                    }
                }
            };
            return jsonSettings;
        }
    }
}
