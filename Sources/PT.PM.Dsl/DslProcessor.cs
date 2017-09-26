using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;

namespace PT.PM.Dsl
{
    public class DslProcessor : IUstSerializer
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


        public UstFormat DataFormat => UstFormat.Dsl;

        public DslProcessor()
        {
            Parser = new DslAntlrParser();
            UstConverter = new DslUstConverter();
        }

        public Ust Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ParsingException("Pattern value can not be empty.") { IsPattern = true };
            }

            Parser.Logger = Logger;
            UstConverter.Logger = Logger;
            DslParser.PatternContext patternContext = Parser.Parse(data);
            UstConverter.Data = data;

            PatternRootUst patternNode = UstConverter.Convert(patternContext);
            patternNode.Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages);

            var preprocessor = new UstSimplifier();
            preprocessor.Logger = Logger;
            patternNode = (PatternRootUst)preprocessor.Visit(patternNode);

            return patternNode;
        }

        public string Serialize(Ust node)
        {
            throw new NotImplementedException();
        }
    }
}
