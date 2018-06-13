using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class UstJsonSerializer : JsonBaseSerializer<Ust>
    {
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
