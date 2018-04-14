using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using static PT.PM.JavaScriptParseTreeUst.JavaScriptParser;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptAntlrParser: AntlrParser
    {
        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public override Language Language => JavaScript.Language;

        protected override int CommentsChannel => JavaScriptLexer.Hidden;

        protected override IVocabulary Vocabulary => JavaScriptParser.DefaultVocabulary;

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
            => new JavaScriptAntlrParseTree((JavaScriptParser.ProgramContext)syntaxTree);

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            var lexer = new JavaScriptLexer(inputStream);
            if (JavaScriptType != JavaScriptType.Undefined)
            {
                lexer.UseStrictDefault = JavaScriptType == JavaScriptType.Strict;
            }

            return lexer;
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
            => new JavaScriptParser(inputStream);

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
            => ((JavaScriptParser)parser).program();

        protected override string LexerSerializedATN => JavaScriptLexer._serializedATN;

        protected override string ParserSerializedATN => JavaScriptParser._serializedATN;
    }
}
