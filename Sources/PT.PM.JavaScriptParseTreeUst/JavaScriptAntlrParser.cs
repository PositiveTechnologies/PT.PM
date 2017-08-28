using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using PT.PM.JavaScriptParseTreeUst.Parser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParser: AntlrParser
    {
        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public override Language Language => Language.JavaScript;

        protected override int CommentsChannel => JavaScriptLexer.Hidden;

        protected override IVocabulary Vocabulary => JavaScriptParser.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new JavaScriptAntlrParseTree((JavaScriptParser.ProgramContext)syntaxTree);
        }

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            var lexer = new JavaScriptLexer(inputStream);
            if (JavaScriptType != JavaScriptType.Undefined)
            {
                lexer.UseStrictDefalult = JavaScriptType == JavaScriptType.Strict;
            }

            return lexer;
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new JavaScriptParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((JavaScriptParser)parser).program();
        }
    }
}
