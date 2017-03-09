using System;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.JavaAstConversion.Parser;
using Antlr4.Runtime;

namespace PT.PM.JavaAstConversion
{
    public class JavaAntlrParser : AntlrParser
    {
        public override Language Language => Language.Java;

        public JavaAntlrParser()
        {
        }

        protected override Lexer InitLexer(ICharStream inputStream)
        {
            return new JavaLexer(inputStream);
        }

        protected override Antlr4.Runtime.Parser InitParser(ITokenStream inputStream)
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
