using System;
using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public class LineColumnTextSpan : IEquatable<LineColumnTextSpan>
    {
        public static LineColumnTextSpan Zero => new LineColumnTextSpan(0, 0, 0, 0, null);

        public int BeginLine { get; set; }

        public int BeginColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public CodeFile CodeFile { get; set; }

        public LineColumnTextSpan()
        {
        }

        public LineColumnTextSpan(int line, int column, CodeFile codeFile = null)
            : this(line, column, line, column, codeFile)
        {
        }

        public LineColumnTextSpan(int beginLine, int beginColumn, int endLine, int endColumn, CodeFile codeFile = null)
        {
            BeginLine = beginLine;
            BeginColumn = beginColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            CodeFile = codeFile;
        }

        public LineColumnTextSpan AddLineOffset(int lineOffset)
        {
            return new LineColumnTextSpan(BeginLine + lineOffset, BeginColumn, EndLine + lineOffset, EndColumn);
        }

        public override string ToString() => ToString(true);

        public string ToString(bool includeFileName)
        {
            string result;

            if (BeginLine == EndLine)
            {
                result = BeginColumn == EndColumn
                    ? $"[{BeginLine},{BeginColumn})"
                    : $"[{BeginLine},{BeginColumn}..{EndColumn})";
            }
            else
            {
                result = BeginColumn == EndColumn
                    ? $"[{BeginLine}..{EndLine},{BeginColumn})"
                    : $"[{BeginLine},{BeginColumn}..{EndLine},{EndColumn})";
            }

            if (includeFileName && CodeFile != null)
            {
                result = $"{result}; {CodeFile}";
            }

            return result;
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
            return CodeFile == other.CodeFile &&
                   BeginLine == other.BeginLine &&
                   BeginColumn == other.BeginColumn &&
                   EndLine == other.EndLine &&
                   EndColumn == other.EndColumn;
        }
    }
}
