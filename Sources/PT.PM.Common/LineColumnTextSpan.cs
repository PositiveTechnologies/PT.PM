using System;

namespace PT.PM.Common
{
    public class LineColumnTextSpan: IEquatable<LineColumnTextSpan>
    {
        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public LineColumnTextSpan()
        {
        }

        public LineColumnTextSpan(int beginLine, int beginColumn, int endLine, int endColumn)
        {
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public override string ToString()
        {
            if (BeginLine == EndLine && EndColumn == BeginColumn + 1)
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
    }
}
