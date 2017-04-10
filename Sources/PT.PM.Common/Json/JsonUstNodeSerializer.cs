using System;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class JsonUstNodeSerializer : IUstNodeSerializer
    {
        private readonly UstJsonConverter astJsonConverter;
        private readonly JsonConverter stringEnumConverter;

        public bool IncludeTextSpans { get; set; } = true;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public UstNodeSerializationFormat DataFormat => UstNodeSerializationFormat.Json;

        public bool Indented { get; set; } = false;

        public bool ExcludeNulls { get; set; } = true;

        public JsonUstNodeSerializer(params Type[] ustNodeAssemblyTypes)
        {
            astJsonConverter = new UstJsonConverter(ustNodeAssemblyTypes);
            stringEnumConverter = new StringEnumConverter();
        }

        public UstNode Deserialize(string data, LanguageFlags sourceLanguage)
        {
            return JsonConvert.DeserializeObject<UstNode>(data, astJsonConverter, stringEnumConverter);
        }

        public string Serialize(UstNode node)
        {
            Formatting indent = Indented ? Formatting.Indented : Formatting.None;
            var converters = new List<JsonConverter>() { stringEnumConverter };
            var jsonSettings = new JsonSerializerSettings()
            {
                Converters = converters,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            if (ExcludeNulls)
            {
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            }
            jsonSettings.ContractResolver = new TextSpanResolver() { Ignore = !IncludeTextSpans };
            return JsonConvert.SerializeObject(node, indent, jsonSettings);
        }
    }
}
