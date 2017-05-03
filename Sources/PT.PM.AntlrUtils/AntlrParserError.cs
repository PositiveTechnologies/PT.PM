using Antlr4.Runtime;

namespace PT.PM.AntlrUtils
{
    public class AntlrParserError : AntlrError
    {
        public IToken Token { get; set; }

        public AntlrParserError(IToken token, int line, int column, string message, RecognitionException exception)
            : base(line, column, message, exception)
        {
            Token = token;
        }
    }
}
