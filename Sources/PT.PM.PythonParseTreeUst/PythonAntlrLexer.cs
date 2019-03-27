using Antlr4.Runtime;
using PT.PM.AntlrUtils;
using PT.PM.Common;
using PythonParseTree;

namespace PT.PM.PythonParseTreeUst
{
    public class PythonAntlrLexer : AntlrLexer
    {
        public override Language Language => Language.Python;
        protected override string LexerSerializedATN => PythonLexer._serializedATN;
        public override IVocabulary Vocabulary => PythonLexer.DefaultVocabulary;
        public override Lexer InitLexer(ICharStream inputStream)
            => new PythonLexer(inputStream);

        public static PythonAntlrLexer Create() => new PythonAntlrLexer();
    }
}