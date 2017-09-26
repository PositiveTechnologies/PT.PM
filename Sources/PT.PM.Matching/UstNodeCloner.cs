using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching
{
    public static class UstNodeCloner
    {
        private static readonly JsonConverter ustJsonConverter = new UstJsonConverter();
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        /// <summary>
        /// TODO: possible replace with binary serialization or protobuf.net for performance reason.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Ust Clone(this Ust source)
        {
            var json = JsonConvert.SerializeObject(source, stringEnumConverter);
            var result = JsonConvert.DeserializeObject<Ust>(json, ustJsonConverter);
            result.FillAscendants();
            return result;
        }
    }
}
