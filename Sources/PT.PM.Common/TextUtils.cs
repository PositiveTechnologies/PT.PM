using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PT.PM.Common
{
    public static class TextUtils
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

            return i;
        }

        public static void ToLineColumn(this TextSpan textSpan, string text, out int startLine, out int startColumn, out int endLine, out int endColumn)
        {
            textSpan.Start.ToLineColumn(text, out startLine, out startColumn);
            textSpan.End.ToLineColumn(text, out endLine, out endColumn);
        }

        public static void ToLineColumn(this int index, string text, out int line, out int column)
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

        public static int GetLinesCount(this string text)
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

        public static int LastIndexOf(this string str, int index, bool whitespace)
        {
            int i = index;
            while (i >= 0 && (whitespace ? char.IsWhiteSpace(str[i]) : !char.IsWhiteSpace(str[i])))
                i--;
            return i;
        }

        public static int FirstIndexOf(this  string str, int index, bool whitespace)
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

        public static TextSpan Union(this IEnumerable<TextSpan> textSpans)
        {
            if (textSpans.Count() == 0)
            {
                return TextSpan.Empty;
            }

            var resultTextSpan = textSpans.First();
            if (textSpans.Count() == 1)
            {
                return resultTextSpan;
            }

            foreach (TextSpan textSpan in textSpans.Skip(1))
            {
                resultTextSpan = resultTextSpan.Union(textSpan);
            }
            return resultTextSpan;
        }
    }
}
