using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;
using System.Linq;
using PT.PM.Patterns.Nodes;
using PT.PM.UstPreprocessing;
using System.Collections.Generic;
using PT.PM.Common.Exceptions;

namespace PT.PM.Dsl
{
    public class DslProcessor : IUstNodeSerializer
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
                UstConverter.Logger = logger;
            }
        }

        protected DslAntlrParser Parser { get; set; }

        protected DslUstConverter UstConverter { get; set; }

        public bool PatternExpressionInsideStatement
        {
            get
            {
                return UstConverter?.PatternExpressionInsideStatement ?? false;
            }
            set
            {
                if (UstConverter != null)
                {
                    UstConverter.PatternExpressionInsideStatement = value;
                }
            }
        }


        public UstNodeSerializationFormat DataFormat => UstNodeSerializationFormat.Dsl;

        public DslProcessor()
        {
            Parser = new DslAntlrParser();
            UstConverter = new DslUstConverter();
        }

        public UstNode Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ParsingException("Pattern value can not be empty.") { IsPattern = true };
            }

            Parser.Logger = Logger;
            UstConverter.Logger = Logger;
            DslParser.PatternContext patternContext = Parser.Parse(data);
            UstConverter.Data = data;

            PatternRootNode patternNode = UstConverter.Convert(patternContext);
            patternNode.Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);

            var preprocessor = new UstSimplifier();
            preprocessor.Logger = Logger;
            patternNode = (PatternRootNode)preprocessor.Visit(patternNode);

            return patternNode;
        }

        public string Serialize(UstNode node)
        {
            throw new NotImplementedException();
        }
    }
}
