using Esprima;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Text.RegularExpressions;
using PT.PM.Common.Files;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaErrorHandler : IErrorHandler, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public TextFile SourceFile { get; }

        public string Source { get; set; }

        public bool Tolerant { get; set; } = true;

        public int LastErrorIndex { get; internal set; } = -1;

        public int Offset { get; set; }

        public Scanner Scanner { get; internal set; }

        public TextFile OriginFile { get; internal set; }

        public JavaScriptEsprimaErrorHandler(TextFile sourceFile)
        {
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
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

        public ParserException CreateError(int index, int line, int column, string message)
        {
            ParserException result = null;

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
                var exception = CreateException(index, message);
                Logger.LogError(exception);
                result = new ParserException(exception.ToString(), exception);
            }

            LastErrorIndex = index;

            return result;
        }

        public void RecordError(ParserException error)
        {
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
            var sourceFile = OriginFile ?? SourceFile;
            TextSpan textSpan;
            try
            {
                textSpan = new TextSpan(index + Offset, 0);
            }
            catch
            {
                textSpan = TextSpan.Zero;
            }
            message = sourceFile.GetLineColumnTextSpan(textSpan) + "; " + Regex.Replace(message, @"Line \d+': ", "");
            return new ParsingException(sourceFile, message: message)
            {
                TextSpan = textSpan
            };
        }
    }
}
