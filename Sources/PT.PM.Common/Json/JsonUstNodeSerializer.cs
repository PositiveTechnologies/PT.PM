using System;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM.Common
{
    public class JsonAstNodeSerializer : IAstNodeSerializer
    {
        private readonly UstJsonConverter astJsonConverter;
        private readonly JsonConverter stringEnumConverter;

        public bool IncludeTextSpans { get; set; } = true;

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public UstNodeSerializationFormat DataFormat => UstNodeSerializationFormat.Json;

        public bool Indented { get; set; }

        public bool ExcludeNulls { get; set; }

        public JsonAstNodeSerializer(params Type[] astNodeAssemblyTypes)
        {
            astJsonConverter = new UstJsonConverter(astNodeAssemblyTypes);
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
                Converters = converters
            };
            if (ExcludeNulls)
            {
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            }
            if (!IncludeTextSpans)
            {
                jsonSettings.ContractResolver = new ShouldNotSerializeTextSpanResolver();
            }
            return JsonConvert.SerializeObject(node, indent, jsonSettings);
        }
    }
}
