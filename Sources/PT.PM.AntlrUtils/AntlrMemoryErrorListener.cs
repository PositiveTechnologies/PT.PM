using Antlr4.Runtime;
using PT.PM.Common;
using PT.PM.Common.Exceptions;

namespace PT.PM.AntlrUtils
{
    public class AntlrMemoryErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        private const int MaxErrorCodeLength = 200;
        private const string ErrorCodeSplitter = " ... ";

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public SourceCodeFile SourceCodeFile { get; set; }

        public bool IsPattern { get; set; }

        public int LineOffset { get; set; }

        public AntlrMemoryErrorListener()
        {
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            line = line - 1 + SourceCodeFile.StartLine;
            charPositionInLine = charPositionInLine + SourceCodeFile.StartColumn;

            var error = new AntlrLexerError(offendingSymbol, line, charPositionInLine, msg, e);
            string errorText = FixLineNumber(error.ToString(), line, charPositionInLine);
            int start = SourceCodeFile.GetLinearFromLineColumn(line, charPositionInLine);
            Logger.LogError(new ParsingException(SourceCodeFile.RelativeName, message: errorText)
                { TextSpan = new TextSpan(start, 1), IsPattern = IsPattern });
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            line = line - 1 + SourceCodeFile.StartLine;
            charPositionInLine = charPositionInLine + SourceCodeFile.StartColumn;

            var error = new AntlrParserError(offendingSymbol, line, charPositionInLine, msg, e);
            string errorText = FixLineNumber(error.ToString(), line, charPositionInLine);
            int start = SourceCodeFile.GetLinearFromLineColumn(line, charPositionInLine);
            Logger.LogError(new ParsingException(SourceCodeFile.RelativeName, message: errorText)
                { TextSpan = new TextSpan(start, 1), IsPattern = IsPattern });
        }

        private string FixLineNumber(string errorText, int line, int charPositionInLine)
        {
            if (LineOffset != 0)
            {
                int atLastIndexOf = errorText.LastIndexOf("at");
                if (atLastIndexOf != -1)
                {
                    errorText = errorText.Remove(atLastIndexOf) + $"at {LineOffset + line}:{charPositionInLine}";
                }
            }

            return errorText;
        }
    }
}
