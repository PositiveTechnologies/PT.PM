using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class JsonUstSerializer : JsonBaseSerializer<Ust>
    {
        public override Ust Deserialize(string data)
        {
            var result = base.Deserialize(data);
            result.FillAscendants(); // TODO: fill ascendats during deserialization
            return result;
        }

        protected override JsonConverterBase CreateConverterBase() => new UstJsonConverter();
    }
}
