using PT.PM.Common.Files;
using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class UstJsonSerializer : JsonBaseSerializer<Ust>
    {
        protected override UstJsonConverterReader CreateConverterReader(TextFile serializedFile)
        {
            return new UstJsonConverterReader(serializedFile);
        }

        protected override UstJsonConverterWriter CreateConverterWriter()
        {
            return new UstJsonConverterWriter();
        }
    }
}
