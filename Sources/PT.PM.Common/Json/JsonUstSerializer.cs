using PT.PM.Common.Nodes;

namespace PT.PM.Common.Json
{
    public class JsonUstSerializer : JsonBaseSerializer<Ust>
    {
        protected override JsonConverterBase CreateConverterBase() => new UstJsonConverter();

        public override Ust Deserialize(string data)
        {
            var result = base.Deserialize(data);
            result.FillAscendants();
            return result;
        }
    }
}
