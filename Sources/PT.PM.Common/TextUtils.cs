using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public static class TextUtils
    {
        private static char[] semicolon = new char[] { ';' };

        private const int StartLine = 1;
        private const int StartColumn = 1;

        public static readonly Regex HttpRegex = new Regex("^https?://", RegexOptions.Compiled);

        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
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
            if (textSpans == null || textSpans.Count() == 0)
            {
                return TextSpan.Zero;
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

        public static TextSpan ParseTextSpan(string text, CodeFile currentCodeFile = null, List<CodeFile> codeFiles = null)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            CodeFile codeFile = GetCodeFile(fileName, currentCodeFile, codeFiles);

            TextSpan result;
            string range = parts[0].Trim().Substring(1, parts[0].Length - 2);
            int index = range.IndexOf("..");
            if (index != -1)
            {
                int start = int.Parse(range.Remove(index));
                int end = int.Parse(range.Substring(index + 2));
                result = TextSpan.FromBounds(start, end, codeFile);
            }
            else
            {
                result = new TextSpan(int.Parse(range), 0, codeFile);
            }

            return result;
        }

        public static LineColumnTextSpan ParseLineColumnTextSpan(string text, CodeFile currentCodeFile = null, List<CodeFile> codeFiles = null)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            CodeFile codeFile = GetCodeFile(fileName, currentCodeFile, codeFiles);

            LineColumnTextSpan result;
            string firstPart = parts[0].Trim();
            var hyphenIndex = firstPart.IndexOf('-');
            if (hyphenIndex != -1)
            {
                ParseLineColumn(firstPart.Remove(hyphenIndex), out int begingLine, out int beginColumn);
                ParseLineColumn(firstPart.Substring(hyphenIndex + 1), out int endLine, out int endColumn);
                result = new LineColumnTextSpan(begingLine, beginColumn, endLine, endColumn, codeFile);
            }
            else
            {
                ParseLineColumn(firstPart, out int line, out int column);
                result = new LineColumnTextSpan(line, column, line, column, codeFile);
            }

            return result;
        }

        public static CodeFile GetCodeFile(string fileName, CodeFile currentCodeFile, List<CodeFile> codeFiles)
        {
            CodeFile result = null;

            if (fileName == null)
            {
                result = currentCodeFile;
            }
            else
            {
                result = codeFiles?.FirstOrDefault(codeFile => codeFile.RelativeName == fileName || codeFile.FullName == fileName);
            }

            return result;
        }

        public static string Substring(this string str, TextSpan textSpan)
        {
            return str.Substring(textSpan.Start, textSpan.Length);
        }

        public static void PadLeft(this StringBuilder builder, int totalWidth)
        {
            builder.Append(' ', totalWidth);
        }

        public static string RemoveWhitespaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            var result = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if (!char.IsWhiteSpace(c))
                    result.Append(c);
            }
            return result.ToString();
        }

        private static void ParseLineColumn(string text, out int line, out int column)
        {
            text = text.Substring(1, text.Length - 2);
            int commaIndex = text.IndexOf(',');
            line = int.Parse(text.Remove(commaIndex));
            column = int.Parse(text.Substring(commaIndex + 1));
        }
    }
}
