using PT.PM.Common;
using PT.PM.Common.Json;

namespace PT.PM.Matching.Json
{
    public class JsonPatternSerializer : JsonBaseSerializer<PatternRoot>, IPatternSerializer
    {
        public string Format => "Json";

        protected override JsonConverterBase CreateConverterBase() => new PatternJsonConverter();

        public override PatternRoot Deserialize(string data)
        {
            var result = base.Deserialize(data);
            var filler = new PatternAscendantsFiller(result);
            filler.FillAscendants();
            return result;
        }
    }
}
