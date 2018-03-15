using System;

namespace PT.PM.Common
{
    public class LineColumnTextSpan: IEquatable<LineColumnTextSpan>
    {
        public static LineColumnTextSpan Zero => new LineColumnTextSpan(0, 0, 0, 0, null);

        private static char[] semicolon = new char[] { ';' };

        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public string FileName { get; set; }

        public LineColumnTextSpan()
        {
        }

        public LineColumnTextSpan(int line, int column, string fileName = null)
            : this(line, column, line, column, fileName)
        {
        }

        public LineColumnTextSpan(int beginLine, int beginColumn, int endLine, int endColumn, string fileName = null)
        {
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            FileName = fileName;
        }

        public static LineColumnTextSpan Parse(string text)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            LineColumnTextSpan result;
            string firstPart = parts[0].Trim();
            var hyphenIndex = firstPart.IndexOf('-');
            if (hyphenIndex != -1)
            {
                ParseLineColumn(firstPart.Remove(hyphenIndex), out int begingLine, out int beginColumn);
                ParseLineColumn(firstPart.Substring(hyphenIndex + 1), out int endLine, out int endColumn);
                result = new LineColumnTextSpan(begingLine, beginColumn, endLine, endColumn, fileName);
            }
            else
            {
                ParseLineColumn(firstPart, out int line, out int column);
                result = new LineColumnTextSpan(line, column, line, column, fileName);
            }

            return result;
        }

        public override string ToString()
        {
            if (BeginLine == EndLine && EndColumn == BeginColumn)
            {
                return $"[{BeginLine},{BeginColumn})";
            }
            return $"[{BeginLine},{BeginColumn}]-[{EndLine},{EndColumn})";
        }

        public override int GetHashCode()
        {
            int result = Hash.Combine(BeginLine, BeginColumn);
            result = Hash.Combine(result, EndLine);
            result = Hash.Combine(result, EndColumn);
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is LineColumnTextSpan && Equals((LineColumnTextSpan)obj);
        }

        public bool Equals(LineColumnTextSpan other)
        {
            return FileName == other.FileName &&
                   BeginLine == other.BeginLine &&
                   BeginColumn == other.BeginColumn &&
                   EndLine == other.EndLine &&
                   EndColumn == other.EndColumn;
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
