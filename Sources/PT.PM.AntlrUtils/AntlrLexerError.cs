using Antlr4.Runtime;

namespace PT.PM.AntlrUtils
{
    public class AntlrLexerError : AntlrError
    {
        public int Lexem { get; set; }

        public AntlrLexerError(int lexem, int line, int column, string message, RecognitionException exception)
            : base(line, column, message, exception)
        {
            Lexem = lexem;
        }
    }
}
