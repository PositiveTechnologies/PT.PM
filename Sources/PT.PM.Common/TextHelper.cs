using System;
using System.IO;

namespace PT.PM.Common
{
    public static class TextHelper
    {
        private const int StartLine = 1;
        private const int StartColumn = 1;
        private const int MaxMessageLength = 200;
        private const double TruncMessageStartRatio = 0.5;
        private const string TruncMessageDelimiter = " ... ";
        private const bool TruncMessageCutWords = false;

        public static int LineColumnToLinear(string text, int line, int column)
        {
            int currentLine = StartLine;
            int currentColumn = StartColumn;

            int i = 0;
            try
            {
                while (currentLine != line || currentLine == line && currentColumn != column)
                {
                    // General line endings:
                    //  Windows: '\r\n'
                    //  Mac (OS 9-): '\r'
                    //  Mac (OS 10+): '\n'
                    //  Unix/Linux: '\n'

                    switch (text[i])
                    {
                        case '\r':
                            currentLine++;
                            currentColumn = StartColumn;
                            if (i + 1 < text.Length && text[i + 1] == '\n')
                            {
                                i++;
                            }
                            break;

                        case '\n':
                            currentLine++;
                            currentColumn = StartColumn;
                            break;

                        default:
                            currentColumn++;
                            break;
                    }

                    i++;
                }
            }
            catch
            {
            }

            return i;
        }

        public static void ToLineColumn(this TextSpan textSpan, string text, out int startLine, out int startColumn, out int endLine, out int endColumn)
        {
            LinearToLineColumn(textSpan.Start, text, out startLine, out startColumn);
            LinearToLineColumn(textSpan.End, text, out endLine, out endColumn);
        }

        public static void LinearToLineColumn(int index, string text, out int line, out int column)
        {
            line = StartLine;
            column = StartColumn;

            int i = 0;
            while (i != index)
            {
                switch (text[i])
                {
                    case '\r':
                        line++;
                        column = StartColumn;
                        if (i + 1 < text.Length && text[i + 1] == '\n')
                        {
                            i++;
                        }
                        break;

                    case '\n':
                        line++;
                        column = StartColumn;
                        break;

                    default:
                        column++;
                        break;
                }
                i++;
            }
        }

        public static int GetLinesCount(string text)
        {
            int result = 1;
            int length = text.Length;
            int i = 0;
            while (i < length)
            {
                if (text[i] == '\r')
                {
                    result++;
                    if (i + 1 < length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else if (text[i] == '\n')
                {
                    result++;
                }
                i++;
            }
            return result;
        }

        public static string Trunc(this string message,
            int maxMessageLength = MaxMessageLength,
            double truncMessageStartRatio = TruncMessageStartRatio,
            string truncMessageSplitter = TruncMessageDelimiter,
            bool truncMessageCutWords = TruncMessageCutWords)
        {
            if (message.Length > maxMessageLength)
            {
                int startLength = (int)Math.Round(maxMessageLength * truncMessageStartRatio);
                int endLength;
                if (!truncMessageCutWords)
                {
                    int newStartLength = message.LastIndexOf(message.LastIndexOf(startLength - 1, false), true) + 1;
                    startLength = newStartLength == 0 && !char.IsWhiteSpace(message[0])
                        ? startLength
                        : newStartLength;

                    int endIndex = message.Length - (maxMessageLength - startLength - truncMessageSplitter.Length);
                    int newEndLength = message.Length - message.FirstIndexOf(FirstIndexOf(message, endIndex, false), true);
                    endLength = newEndLength == 0 && !char.IsWhiteSpace(message[message.Length - 1])
                        ? maxMessageLength - startLength - truncMessageSplitter.Length
                        : newEndLength;
                }
                else
                {
                    endLength = maxMessageLength - startLength - truncMessageSplitter.Length;
                }
                return message.Substring(0, startLength) +
                       truncMessageSplitter +
                       message.Substring(message.Length - endLength, endLength);
            }
            return message;
        }

        private static int LastIndexOf(this string str, int index, bool whitespace)
        {
            int i = index;
            while (i >= 0 && (whitespace ? char.IsWhiteSpace(str[i]) : !char.IsWhiteSpace(str[i])))
                i--;
            return i;
        }

        private static int FirstIndexOf(this  string str, int index, bool whitespace)
        {
            int i = index;
            while (i < str.Length && (whitespace ? char.IsWhiteSpace(str[i]) : !char.IsWhiteSpace(str[i])))
                i++;
            return i;
        }

        public static string NormDirSeparator(this string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
