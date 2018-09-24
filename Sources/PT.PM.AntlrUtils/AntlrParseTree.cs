using PT.PM.Common;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParseTree : ParseTree
    {
        public IList<IToken> Tokens;

        public ParserRuleContext SyntaxTree { get; }

        public IList<IToken> Comments = new List<IToken>();

        protected AntlrParseTree(ParserRuleContext syntaxTree)
        {
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
        }

        public AntlrParseTree()
        {
        }
    }
}
