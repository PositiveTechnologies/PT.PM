using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class JsonUstSerializer : JsonBaseSerializer<Ust>
    {
        public override Ust Deserialize(CodeFile jsonFile)
        {
            var result = base.Deserialize(jsonFile);
            result.FillAscendants(); // TODO: fill ascendats during deserialization
            return result;
        }

        protected override JsonConverterBase CreateConverterBase(CodeFile jsonFile) => new UstJsonConverter(JsonFile);
    }
}
