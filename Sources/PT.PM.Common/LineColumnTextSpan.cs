using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public class LineColumnTextSpan: IEquatable<LineColumnTextSpan>
    {
        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public LineColumnTextSpan(int beginLine, int beginColumn, int endLine, int endColumn)
        {
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        /// <summary>
        /// TODO: move implementation to SourceCodeFile.
        /// </summary>
        public LineColumnTextSpan(TextSpan textSpan, string text)
        {
            int beginLine, beginColumn, endLine, endColumn;
            TextHelper.LinearToLineColumn(textSpan.Start, text, out beginLine, out beginColumn);
            TextHelper.LinearToLineColumn(textSpan.End, text, out endLine, out endColumn);
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public override string ToString()
        {
            return $"[{BeginLine};{BeginColumn})-[{EndLine};{EndColumn})";
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
            return BeginLine == other.BeginLine && BeginColumn == other.BeginColumn &&
                   EndLine == other.EndLine && EndColumn == other.EndColumn;
        }
    }
}
