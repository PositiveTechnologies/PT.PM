using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaParseTreeUst.Parser;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrParser : AntlrParser
    {
        public override Language Language => Java.Language;

        public JavaAntlrParser()
        {
        }

        public override Lexer InitLexer(ICharStream inputStream)
        {
            return new JavaLexer(inputStream);
        }

        public override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
        {
            return new JavaParser(inputStream);
        }

        protected override ParserRuleContext Parse(Antlr4.Runtime.Parser parser)
        {
            return ((JavaParser)parser).compilationUnit();
        }

        protected override AntlrParseTree Create(ParserRuleContext syntaxTree)
        {
            return new JavaAntlrParseTree((JavaParser.CompilationUnitContext)syntaxTree);
        }

        protected override IVocabulary Vocabulary
        {
            get
            {
                return JavaLexer.DefaultVocabulary;
            }
        }

        protected override int CommentsChannel
        {
            get
            {
                return JavaLexer.Hidden;
            }
        }
    }
}
