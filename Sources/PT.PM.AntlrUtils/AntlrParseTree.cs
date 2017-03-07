using PT.PM.Common;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParseTree : ParseTree
    {
        public readonly ParserRuleContext SyntaxTree;

        public TimeSpan LexerTimeSpan;

        public TimeSpan ParserTimeSpan;

        public IList<IToken> Tokens = new List<IToken>();

        public IList<IToken> Comments = new List<IToken>();

        protected AntlrParseTree(ParserRuleContext syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public AntlrParseTree()
        {
        }
    }
}
