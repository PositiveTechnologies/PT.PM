using System;

namespace PT.PM.Common
{
    public class LineColumnTextSpan: IEquatable<LineColumnTextSpan>
    {
        public static LineColumnTextSpan Empty => new LineColumnTextSpan(0, 0, 0, 0);

        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public LineColumnTextSpan()
        {
        }

        public LineColumnTextSpan(int line, int column)
            : this(line, column, line, column)
        {
        }

        public LineColumnTextSpan(int beginLine, int beginColumn, int endLine, int endColumn)
        {
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public static LineColumnTextSpan Parse(string text)
        {
            LineColumnTextSpan result;
            var hyphenIndex = text.IndexOf('-');
            if (hyphenIndex != -1)
            {
                ParseLineColumn(text.Remove(hyphenIndex), out int begingLine, out int beginColumn);
                ParseLineColumn(text.Substring(hyphenIndex + 1), out int endLine, out int endColumn);
                result = new LineColumnTextSpan(begingLine, beginColumn, endLine, endColumn);
            }
            else
            {
                ParseLineColumn(text, out int line, out int column);
                result = new LineColumnTextSpan(line, column, line, column);
            }
            return result;
        }

        public override string ToString()
        {
            if (BeginLine == EndLine && EndColumn == BeginColumn)
            {
                return $"[{BeginLine};{BeginColumn})";
            }
            return $"[{BeginLine};{BeginColumn}]-[{EndLine};{EndColumn})";
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
            return BeginLine == other.BeginLine &&
                   BeginColumn == other.BeginColumn &&
                   EndLine == other.EndLine &&
                   EndColumn == other.EndColumn;
        }

        private static void ParseLineColumn(string text, out int line, out int column)
        {
            text = text.Substring(1, text.Length - 2);
            int semicolonIndex = text.IndexOf(';');
            line = int.Parse(text.Remove(semicolonIndex));
            column = int.Parse(text.Substring(semicolonIndex + 1));
        }
    }
}
