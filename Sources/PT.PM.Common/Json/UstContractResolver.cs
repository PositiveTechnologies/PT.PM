using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;

namespace PT.PM.Common.Json
{
    public class UstContractResolver : DefaultContractResolver
    {
        public JsonConverter UstJsonReader { get; set; }

        public JsonConverter CodeFileConverter { get; set; }

        public JsonConverter TextSpanConverter { get; set; }

        public JsonConverter LanguageConverter { get; set; }



        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            if (objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust)))
            {
                contract.Converter = UstJsonReader;
            }
            else if (objectType == typeof(CodeFile))
            {
                contract.Converter = CodeFileConverter;
            }
            else if (objectType == typeof(Language))
            {
                contract.Converter = LanguageConverter;
            }
            else if (objectType == typeof(TextSpan))
            {
                contract.Converter = TextSpanConverter;
            }

            return contract;
        }
    }
}
