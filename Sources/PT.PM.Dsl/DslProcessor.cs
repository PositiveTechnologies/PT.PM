using PT.PM.Common;
using PT.PM.Common.Exceptions;
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

        public PatternRoot Deserialize(string data)
        {
            var codeFile = new CodeFile(data) { Name = "Pattern", IsPattern = true };
            if (string.IsNullOrEmpty(data))
            {
                throw new ParsingException(codeFile, message: "Pattern value can not be empty.");
            }

            var parser = new DslAntlrParser() { Logger = Logger };
            var converter = new DslUstConverter
            {
                Logger = Logger,
                PatternExpressionInsideStatement = PatternExpressionInsideStatement,
                Data = codeFile
            };
            DslParser.PatternContext patternContext = parser.Parse(data);

            PatternRoot patternNode = converter.Convert(patternContext);
            patternNode.CodeFile = codeFile;

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
