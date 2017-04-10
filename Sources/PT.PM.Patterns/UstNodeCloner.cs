using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Patterns.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PT.PM.Patterns
{
    public static class UstNodeCloner
    {
        private static readonly JsonConverter astJsonConverter = new UstJsonConverter(typeof(UstNode), typeof(PatternVarDef));
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        /// <summary>
        /// TODO: possible replace with binary serialization or protobuf.net for performance reason.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static UstNode Clone(this UstNode source)
        {
            var json = JsonConvert.SerializeObject(source, stringEnumConverter);
            var result = JsonConvert.DeserializeObject<UstNode>(json, astJsonConverter);
            return result;
        }
    }
}
