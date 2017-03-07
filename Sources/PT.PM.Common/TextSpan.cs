using System;
using Newtonsoft.Json;
using System.Collections;

namespace PT.PM.Common
{
    /// <summary>
    /// Source: Roslyn, http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/Text/TextSpan.cs
    /// </summary>
    public struct TextSpan: IEquatable<TextSpan>, IComparable<TextSpan>
    {
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

        public int End => Start + Length;

        public int Length { get; }

        public bool IsEmpty => this.Length == 0;

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

            return TextSpan.FromBounds(unionStart, unionEnd);
        }

        public TextSpan AddOffset(int offset)
        {
            return new TextSpan(Start + offset, Length);
        }

        public static TextSpan FromBounds(int start, int end)
        {
            return new TextSpan(start, end - start);
        }

        public static TextSpan FromTextAndLineColumn(string text, int startLine, int startColumn, int endLine, int endColumn)
        {
            int start = TextHelper.LineColumnToLinear(text, startLine, startColumn);
            int end = TextHelper.LineColumnToLinear(text, endLine, endColumn);
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
