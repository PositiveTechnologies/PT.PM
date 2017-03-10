using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using PT.PM.JavaScriptParseTreeUst.Parser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParser: AntlrParser
    {
        public override Language Language => Language.JavaScript;

        protected override int CommentsChannel => ECMAScriptLexer.Hidden;

        protected override IVocabulary Vocabulary => ECMAScriptParser.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new JavaScriptAntlrParseTree((ECMAScriptParser.ProgramContext)syntaxTree);
        }

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            return new ECMAScriptLexer(inputStream);
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new ECMAScriptParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((ECMAScriptParser)parser).program();
        }
    }
}
