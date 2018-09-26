using Esprima;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaErrorHandler : IErrorHandler, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CodeFile CodeFile { get; }

        public string Source { get; set; }

        public bool Tolerant { get; set; } = true;

        public int LastErrorIndex { get; internal set; } = -1;

        public int Offset { get; set; }

        public Scanner Scanner { get; internal set; }

        public CodeFile OriginFile { get; internal set; }

        public JavaScriptEsprimaErrorHandler(CodeFile codeFile)
        {
            CodeFile = codeFile ?? throw new ArgumentNullException(nameof(codeFile));
        }

        public void ThrowError(int index, int line, int column, string message)
        {
            if (LastErrorIndex == index)
            {
                index++;
                if (Scanner != null)
                {
                    Scanner.Index = index;
                }
            }
            else
            {
                Logger.LogError(CreateException(index, message));
            }

            LastErrorIndex = index;
        }

        public void Tolerate(ParserException error)
        {
            if (Tolerant)
            {
                RecordError(error);
            }
            else
            {
                throw CreateException(error.Index, error.Message);
            }
        }

        public void RecordError(ParserException error)
        {
            Logger.LogError(CreateException(error.Index, error.Message));
        }

        public void TolerateError(int index, int line, int column, string message)
        {
            ParsingException parsingException = CreateException(index, message);

            if (Tolerant)
            {
                Logger.LogError(parsingException);
            }
            else
            {
                throw parsingException;
            }
        }

        public ParsingException CreateException(int index, string message)
        {
            var codeFile = OriginFile ?? CodeFile;
            TextSpan textSpan = new TextSpan(index + Offset, 0);
            message = codeFile.GetLineColumnTextSpan(textSpan) + "; " + Regex.Replace(message, @"Line \d+': ", "");
            return new ParsingException(codeFile, message: message)
            {
                TextSpan = textSpan
            };
        }
    }
}
