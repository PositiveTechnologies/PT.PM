using System;

namespace PT.PM.Common
{
    public struct TextSpan: IEquatable<TextSpan>, IComparable<TextSpan>, IComparable
    {
        public readonly static TextSpan Zero = default(TextSpan);

        public TextSpan(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (start + length < start)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Start = start;
            Length = length;
            CodeFile = null;
        }

        public TextSpan(TextSpan textSpan)
        {
            Start = textSpan.Start;
            Length = textSpan.Length;
            CodeFile = textSpan.CodeFile;
        }

        public int Start { get; }

        public int Length { get; }

        public CodeFile CodeFile { get; set; }

        public int End => Start + Length;

        public bool IsZero => Start == 0 && Length == 0;

        public TextSpan Union(TextSpan span)
        {
            if (CodeFile != span.CodeFile)
            {
                return IsZero ? span : this;
            }

            if (IsZero)
            {
                return span;
            }

            if (span.IsZero)
            {
                return this;
            }

            int unionStart = Math.Min(Start, span.Start);
            int unionEnd = Math.Max(End, span.End);

            return FromBounds(unionStart, unionEnd);
        }

        public TextSpan AddOffset(int offset)
        {
            return new TextSpan(Start + offset, Length);
        }

        public static TextSpan FromBounds(int start, int end)
        {
            return new TextSpan(start, end - start);
        }

        public static TextSpan Parse(string text)
        {
            string range = text.Substring(1, text.Length - 2);
            int index = range.IndexOf("..");
            if (index != -1)
            {
                int start = int.Parse(range.Remove(index));
                int end = int.Parse(range.Substring(index + 2));
                return FromBounds(start, end);
            }
            else
            {
                return new TextSpan(int.Parse(range), 0);
            }
        }

        public static bool operator ==(TextSpan left, TextSpan right) => left.Equals(right);

        public static bool operator !=(TextSpan left, TextSpan right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            return obj is TextSpan && Equals((TextSpan)obj);
        }

        public bool Equals(TextSpan other)
        {
            if (CodeFile != other.CodeFile)
            {
                return false;
            }

            return Start == other.Start && Length == other.Length;
        }

        public override int GetHashCode()
        {
            int result = Hash.Combine(Start, Length);

            if (!ReferenceEquals(CodeFile, null))
            {
                result = Hash.Combine(CodeFile.GetHashCode(), result);
            }

            return result;
        }

        public override string ToString()
        {
            string result = Start == End
                ? $"[{Start})"
                : $"[{Start}..{End})";

            if (!ReferenceEquals(CodeFile, null))
            {
                result = $"{result}; {CodeFile.RelativeName}";
            }

            return result;
        }

        public int CompareTo(object obj)
        {
            if (obj is TextSpan otherTextSpan)
            {
                return CompareTo(otherTextSpan);
            }

            return 1;
        }

        public int CompareTo(TextSpan other)
        {
            if (CodeFile != other.CodeFile)
            {
                return CodeFile != null
                    ? CodeFile.CompareTo(other.CodeFile)
                    : 1;
            }

            int diff = Start - other.Start;
            if (diff != 0)
            {
                return diff;
            }

            return Length - other.Length;
        }
    }
}
