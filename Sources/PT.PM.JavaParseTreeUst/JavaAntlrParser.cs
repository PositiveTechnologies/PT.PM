using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParser : AntlrParser
    {
        public override Language Language => Java.Language;

        public JavaAntlrParser()
        {
        }

        protected override Lexer InitLexer(ICharStream inputStream) =>
            new JavaLexer(inputStream);

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream) =>
            new JavaParser(inputStream);

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser) =>
            ((JavaParser)parser).compilationUnit();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new JavaAntlrParseTree((JavaParser.CompilationUnitContext)syntaxTree);

        protected override IVocabulary Vocabulary => JavaLexer.DefaultVocabulary;

        protected override int CommentsChannel => JavaLexer.Hidden;

        protected override string LexerSerializedATN => JavaLexer._serializedATN;

        protected override string ParserSerializedATN => JavaParser._serializedATN;
    }
}
