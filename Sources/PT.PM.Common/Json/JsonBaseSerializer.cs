using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM.Common.Json
{
    public abstract class JsonBaseSerializer<T>
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        public bool IncludeTextSpans { get; set; } = true;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool Indented { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        protected abstract JsonConverterBase CreateConverterBase();

        public virtual T Deserialize(string data)
        {
            JsonConverterBase baseJsonConverter = CreateConverterBase();
            var result = JsonConvert.DeserializeObject<T>(data, baseJsonConverter, stringEnumConverter);
            return result;
        }

        public virtual string Serialize(T node)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(node, Indented ? Formatting.Indented : Formatting.None, jsonSettings);
        }

        public string Serialize(IEnumerable<T> nodes)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(nodes, Indented ? Formatting.Indented : Formatting.None, jsonSettings);
        }

        private JsonSerializerSettings PrepareSettings()
        {
            JsonConverterBase jsonConverterBase = CreateConverterBase();
            jsonConverterBase.IncludeTextSpans = IncludeTextSpans;
            jsonConverterBase.ExcludeDefaults = ExcludeDefaults;
            var jsonSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    stringEnumConverter,
                    jsonConverterBase
                }
            };
            return jsonSettings;
        }
    }
}
