using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;

namespace PT.PM.Python3ParseTreeUst
{
    public class Python3AntlrLexer : AntlrLexer
    {
        public override Language Language => Language.Python3;
        protected override string LexerSerializedATN => Python3Lexer._serializedATN;
        public override IVocabulary Vocabulary => Python3Lexer.DefaultVocabulary;
        public override Lexer InitLexer(ICharStream inputStream)
            => new Python3Lexer(inputStream);

        public static Python3AntlrLexer Create() => new Python3AntlrLexer();
    }
}