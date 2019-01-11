using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Matching;

namespace PT.PM.Dsl
{
    public class DslProcessor : IPatternSerializer
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Format => "Dsl";

        public bool PatternExpressionInsideStatement { get; set; }

        public DslProcessor()
        {
        }

        public PatternRoot Deserialize(TextFile data)
        {
            if (string.IsNullOrEmpty(data.Data))
            {
                throw new ParsingException(data, message: "Pattern value can not be empty.");
            }

            var parser = new DslAntlrParser() { Logger = Logger };
            var converter = new DslUstConverter
            {
                Logger = Logger,
                PatternExpressionInsideStatement = PatternExpressionInsideStatement,
                Data = data
            };
            DslParser.PatternContext patternContext = parser.Parse(data.PatternKey, data.Data);

            PatternRoot patternNode = converter.Convert(patternContext);
            patternNode.File = data;

            var preprocessor = new PatternNormalizer() { Logger = Logger };
            patternNode = preprocessor.Normalize(patternNode);

            return patternNode;
        }

        public string Serialize(PatternRoot patternRoot)
        {
            return patternRoot.Node.ToString();
        }
    }
}
