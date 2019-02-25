using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.JavaParseTreeUst
{
    public class JavaAntlrLexer : AntlrLexer
    {
        public static JavaAntlrLexer Create() => new JavaAntlrLexer();
        
        protected override IVocabulary Vocabulary => JavaLexer.DefaultVocabulary;

        public override Language Language => Language.Java;

        protected override string LexerSerializedATN => JavaLexer._serializedATN;

        public override Lexer InitLexer(ICharStream inputStream) =>
            new JavaLexer(inputStream);
    }
}