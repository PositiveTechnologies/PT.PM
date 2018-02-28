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
    }
}
