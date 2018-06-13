using PT.PM.Common;
using PT.PM.Common.Json;

namespace PT.PM.Matching.Json
{
    public class JsonPatternSerializer : JsonBaseSerializer<PatternRoot>, IPatternSerializer
    {
        public string Format => "Json";

        protected override UstJsonConverterReader CreateConverterReader(CodeFile jsonFile)
        {
            return new PatternJsonConverterReader(jsonFile);
        }

        protected override UstJsonConverterWriter CreateConverterWriter()
        {
            return new PatternJsonConverterWriter();
        }

        public override PatternRoot Deserialize(CodeFile jsonFile)
        {
            var result = base.Deserialize(jsonFile);
            var filler = new PatternAscendantsFiller(result);
            filler.FillAscendants();
            return result;
        }
    }
}
