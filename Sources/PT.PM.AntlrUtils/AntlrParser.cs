using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using PT.PM.Common;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrParser : AntlrParserBase, ILanguageParser<IList<IToken>>
    {
        protected override bool IsParseTreeFree => false;

        protected abstract AntlrParseTree Create(ParserRuleContext syntaxTree);

        public ParseTree Parse(IList<IToken> tokens, out TimeSpan parserTimeSpan)
        {
            var syntaxTree = (ParserRuleContext)ProcessTokens(tokens, null, out List<IToken> commentTokens, out parserTimeSpan);

            if (syntaxTree == null)
            {
                return null;
            }

            AntlrParseTree parseTree = Create(syntaxTree);
            parseTree.Tokens = tokens;
            parseTree.Comments = commentTokens;
            parseTree.SourceFile = ((LightToken)tokens.First(token => token is LightToken)).TextFile;

            return parseTree;
        }
    }
}