using PT.PM.Common;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.AntlrUtils
{
    public class AntlrMemoryErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        private const int MaxErrorCodeLength = 200;
        private const string ErrorCodeSplitter = " ... ";

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string FileName { get; set; }

        public string FileData { get; set; }

        public AntlrMemoryErrorListener()
        {
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var error = new AntlrLexerError(offendingSymbol, line, charPositionInLine, msg, e);
            int start = TextHelper.LineColumnToLinear(FileData, line, charPositionInLine);
            Logger.LogError(new ParsingException(FileName, error.ToString()) { TextSpan = new TextSpan(start, 1) });
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var error = new AntlrParserError(offendingSymbol, line, charPositionInLine, msg, e);
            var errorText = error.ToString();
            if (errorText.Contains("no viable alternative at input"))
            {
                var firstInd = errorText.IndexOf("'");
                var secondInd = errorText.LastIndexOf("'");
                var errorCode = errorText.Substring(firstInd + 1, secondInd - firstInd - 1);
                if (errorCode.Length > MaxErrorCodeLength + ErrorCodeSplitter.Length)
                {
                    errorCode = errorCode.Substring(0, MaxErrorCodeLength / 2) + ErrorCodeSplitter +
                                errorCode.Substring(errorCode.Length - MaxErrorCodeLength / 2);
                }
                errorText = errorText.Substring(0, firstInd + 1) + errorCode + errorText.Substring(secondInd);
            }
            int start = TextHelper.LineColumnToLinear(FileData, line, charPositionInLine);
            Logger.LogError(new ParsingException(FileName, errorText) { TextSpan = new TextSpan(start, 1) });
        }
    }
}
