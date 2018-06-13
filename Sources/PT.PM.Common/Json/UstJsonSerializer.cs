using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class UstJsonSerializer : JsonBaseSerializer<Ust>
    {
        public override Ust Deserialize(CodeFile jsonFile)
        {
            var result = base.Deserialize(jsonFile);
            result.FillAscendants(); // TODO: fill ascendats during deserialization
            return result;
        }

        protected override UstJsonConverterReader CreateConverterReader(CodeFile jsonFile)
        {
            return new UstJsonConverterReader(jsonFile);
        }

        protected override UstJsonConverterWriter CreateConverterWriter()
        {
            return new UstJsonConverterWriter();
        }
    }
}
