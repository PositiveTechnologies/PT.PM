using PT.PM.Common;
using PT.PM.Common.Json;

namespace PT.PM.Matching.Json
{
    public class JsonPatternSerializer : JsonBaseSerializer<PatternRoot>, IPatternSerializer
    {
        public string Format => "Json";

        protected override JsonConverterBase CreateConverterBase(CodeFile jsonFile) => new PatternJsonConverter(jsonFile);

        public override PatternRoot Deserialize(CodeFile jsonFile)
        {
            var result = base.Deserialize(jsonFile);
            var filler = new PatternAscendantsFiller(result);
            filler.FillAscendants();
            return result;
        }
    }
}
