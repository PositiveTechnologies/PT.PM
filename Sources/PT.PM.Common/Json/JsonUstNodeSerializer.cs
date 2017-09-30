using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class JsonUstSerializer : IUstSerializer
    {
        private readonly UstJsonConverter defaultUstJsonConverter;
        private readonly JsonConverter stringEnumConverter;

        public bool IncludeTextSpans { get; set; } = true;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public UstSerializeFormat DataFormat => UstSerializeFormat.Json;

        public bool Indented { get; set; } = false;

        public bool ExcludeNulls { get; set; } = true;

        public JsonUstSerializer()
        {
            defaultUstJsonConverter = new UstJsonConverter();
            stringEnumConverter = new StringEnumConverter();
        }

        public Ust Deserialize(string data)
        {
            var result = JsonConvert.DeserializeObject<Ust>(data, defaultUstJsonConverter, stringEnumConverter);
            result.FillAscendants();
            return result;
        }

        public string Serialize(Ust node)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(node, Indented ? Formatting.Indented : Formatting.None, jsonSettings);
        }

        public string Serialize(IEnumerable<Ust> nodes)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.SerializeObject(nodes, Indented ? Formatting.Indented : Formatting.None, jsonSettings);
        }

        private JsonSerializerSettings PrepareSettings()
        {
            var ustJsonConverter = new UstJsonConverter { IncludeTextSpans = IncludeTextSpans };
            var jsonSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { stringEnumConverter, ustJsonConverter }
            };
            if (ExcludeNulls)
            {
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            }
            return jsonSettings;
        }
    }
}
