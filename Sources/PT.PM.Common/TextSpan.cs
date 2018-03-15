using System;

namespace PT.PM.Common
{
    public struct TextSpan: IEquatable<TextSpan>, IComparable<TextSpan>, IComparable
    {
        public readonly static TextSpan Zero = default(TextSpan);

        private static char[] semicolon = new char[] { ';' };

        public TextSpan(int start, int length, string fileName = null)
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
            FileName = fileName;
        }

        public TextSpan(TextSpan textSpan)
        {
            Start = textSpan.Start;
            Length = textSpan.Length;
            FileName = textSpan.FileName;
        }

        public int Start { get; }

        public int Length { get; }

        public string FileName { get; set; }

        public int End => Start + Length;

        public bool IsZero => Start == 0 && Length == 0;

        public TextSpan Union(TextSpan span)
        {
            if (FileName != span.FileName)
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
            return new TextSpan(Start + offset, Length, FileName);
        }

        public static TextSpan FromBounds(int start, int end, string fileName = null)
        {
            return new TextSpan(start, end - start, fileName);
        }

        public static TextSpan Parse(string text)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            TextSpan result;
            string range = parts[0].Trim().Substring(1, parts[0].Length - 2);
            int index = range.IndexOf("..");
            if (index != -1)
            {
                int start = int.Parse(range.Remove(index));
                int end = int.Parse(range.Substring(index + 2));
                result = FromBounds(start, end, fileName);
            }
            else
            {
                result = new TextSpan(int.Parse(range), 0, fileName);
            }

            return result;
        }

        public static bool operator ==(TextSpan left, TextSpan right) => left.Equals(right);

        public static bool operator !=(TextSpan left, TextSpan right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            return obj is TextSpan && Equals((TextSpan)obj);
        }

        public bool Equals(TextSpan other)
        {
            if (FileName != other.FileName)
            {
                return false;
            }

            return Start == other.Start && Length == other.Length;
        }

        public override int GetHashCode()
        {
            int result = Hash.Combine(Start, Length);

            if (!(FileName is null))
            {
                result = Hash.Combine(FileName.GetHashCode(), result);
            }

            return result;
        }

        public override string ToString()
        {
            string result = Start == End
                ? $"[{Start})"
                : $"[{Start}..{End})";

            if (!(FileName is null))
            {
                result = $"{result}; {FileName}";
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
            if (FileName != other.FileName)
            {
                return FileName != null
                    ? FileName.CompareTo(other.FileName)
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
