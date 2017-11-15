using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public static class TextUtils
    {
        private const int StartLine = 1;
        private const int StartColumn = 1;

        public static readonly Regex HttpRegex = new Regex("^https?://", RegexOptions.Compiled);

        public static int LineColumnToLinear(string text, int line, int column)
        {
            int currentLine = StartLine;
            int currentColumn = StartColumn;

            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == '\r' || c == '\n')
                {
                    currentLine++;
                    currentColumn = StartColumn;
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    currentColumn++;
                }
                i++;

                if (currentLine == line && currentColumn == column)
                {
                    break;
                }
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
            while (i < text.Length)
            {
                if (i == index)
                {
                    break;
                }

                char c = text[i];
                if (c == '\r' || c == '\n')
                {
                    line++;
                    column = StartColumn;
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    column++;
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

        public static string Substring(this string str, TextSpan textSpan)
        {
            return str.Substring(textSpan.Start, textSpan.Length);
        }

        public static void PadLeft(this StringBuilder builder, int totalWidth)
        {
            builder.Append(' ', totalWidth);
        }
    }
}
