using System;

namespace PT.PM.Common
{
    /// <summary>
    /// Source: Roslyn, http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/Text/TextSpan.cs
    /// </summary>
    public struct TextSpan: IEquatable<TextSpan>, IComparable<TextSpan>, IComparable
    {
        public readonly static TextSpan Empty = default(TextSpan);

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
        }

        public TextSpan(TextSpan textSpan)
        {
            Start = textSpan.Start;
            Length = textSpan.Length;
        }

        public int Start { get; }

        public int Length { get; }

        public int End => Start + Length;

        public bool IsEmpty => Length == 0;

        public bool Contains(int position)
        {
            return unchecked((uint)(position - Start) < (uint)Length);
        }

        public bool Contains(TextSpan span)
        {
            return span.Start >= Start && span.End <= End;
        }

        public bool IntersectsWith(TextSpan span)
        {
            return span.Start <= this.End && span.End >= Start;
        }

        public bool IntersectsWith(int position)
        {
            return unchecked((uint)(position - Start) <= (uint)Length);
        }

        public TextSpan Intersection(TextSpan span)
        {
            int intersectStart = Math.Max(Start, span.Start);
            int intersectEnd = Math.Min(End, span.End);

            return intersectStart <= intersectEnd
                ? TextSpan.FromBounds(intersectStart, intersectEnd)
                : default(TextSpan);
        }

        public TextSpan Union(TextSpan span)
        {
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
            int start = int.Parse(range.Remove(index));
            int end = int.Parse(range.Substring(index + 2));
            return FromBounds(start, end);
        }

        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !left.Equals(right);
        }

        public bool Equals(TextSpan other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            return obj is TextSpan && Equals((TextSpan)obj);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Start, Length);
        }

        public override string ToString()
        {
            return $"[{Start}..{End})"; 
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
            var diff = Start - other.Start;
            if (diff != 0)
            {
                return diff;
            }

            return Length - other.Length;
        }
    }
}
