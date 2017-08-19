using PT.PM.Common;

namespace PT.PM
{
    public class ParserConverterSet
    {
        public ILanguageParser Parser { get; set; }

        public IParseTreeToUstConverter Converter { get; set; }
    }
}
