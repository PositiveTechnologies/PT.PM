using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParser : AntlrParser
    {
        public override Language Language => Language.Java;

        public static JavaAntlrParser Create() => new JavaAntlrParser();

        public JavaAntlrParser()
        {
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream) =>
            new JavaParser(inputStream);

        public override AntlrLexer InitAntlrLexer()
        => new JavaAntlrLexer();

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser) =>
            ((JavaParser) parser).compilationUnit();

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree) =>
            new JavaAntlrParseTree((JavaParser.CompilationUnitContext) syntaxTree);

        protected override int CommentsChannel => JavaLexer.Hidden;

        protected override string ParserSerializedATN => JavaParser._serializedATN;
    }
}