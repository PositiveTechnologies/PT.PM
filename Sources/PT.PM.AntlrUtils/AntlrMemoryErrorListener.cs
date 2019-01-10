using Antlr4.Runtime;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;

namespace PT.PM.AntlrUtils
{
    public class AntlrMemoryErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CodeFile CodeFile { get; set; }

        public bool IsPattern { get; set; }

        public int LineOffset { get; set; }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (recognizer is Lexer lexer)
            {
                ProcessError(lexer.CharIndex, lexer.CharIndex, msg);
            }
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            ProcessError(offendingSymbol.StartIndex, offendingSymbol.StopIndex, msg);
        }

        private void ProcessError(int startIndex, int stopIndex, string msg)
        {
            int lineLinearIndex = CodeFile.GetLineLinearIndex(LineOffset);
            startIndex = startIndex + lineLinearIndex;
            stopIndex = stopIndex + 1 + lineLinearIndex;
            if (stopIndex <= startIndex)
            {
                startIndex = stopIndex - 1;
            }
            TextSpan textSpan = TextSpan.FromBounds(startIndex, stopIndex);

            string errorMessage = $"{msg} at {CodeFile.GetLineColumnTextSpan(textSpan)}";

            Logger.LogError(new ParsingException(CodeFile, message: errorMessage)
            {
                TextSpan = textSpan,
            });
        }
    }
}
