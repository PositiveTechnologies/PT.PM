using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Linq;
using PT.PM.Patterns.Nodes;
using PT.PM.UstPreprocessing;

namespace PT.PM.Dsl
{
    public class DslProcessor : IAstNodeSerializer
    {
        private ILogger logger = DummyLogger.Instance;

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
                Parser.Logger = logger;
                AstConverter.Logger = logger;
            }
        }

        protected DslAntlrParser Parser { get; set; }

        protected DslAstConverter AstConverter { get; set; }

        public bool PatternExpressionInsideStatement
        {
            get
            {
                return AstConverter?.PatternExpressionInsideStatement ?? false;
            }
            set
            {
                if (AstConverter != null)
                {
                    AstConverter.PatternExpressionInsideStatement = value;
                }
            }
        }


        public UstNodeSerializationFormat DataFormat { get { return UstNodeSerializationFormat.Dsl; } }

        public DslProcessor()
        {
            Parser = new DslAntlrParser();
            AstConverter = new DslAstConverter();
        }

        public UstNode Deserialize(string data, LanguageFlags sourceLanguage)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ParsingException("Pattern value can not be empty.");
            }

            Parser.Logger = Logger;
            AstConverter.Logger = Logger;
            DslParser.PatternContext patternContext = Parser.Parse(data);
            AstConverter.SourceLanguage = sourceLanguage;
            AstConverter.Data = data;
            DslNode dslNode = AstConverter.Convert(patternContext);
            UstNode result = dslNode.Collection.First();
            ResultPatternVars = dslNode.PatternVarDefs;
            var preprocessor = new UstPreprocessor();
            preprocessor.Logger = Logger;
            result = new PatternNode(result, dslNode.PatternVarDefs);
            result = preprocessor.Preprocess(result);

            return result;
        }

        public PatternVarDef[] ResultPatternVars { get; private set; }

        public string Serialize(UstNode node)
        {
            throw new NotImplementedException();
        }
    }
}
