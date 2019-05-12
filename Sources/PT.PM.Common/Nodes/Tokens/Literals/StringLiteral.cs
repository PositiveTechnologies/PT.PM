using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class StringLiteral : Literal
    {
        [Key(UstFieldOffset)]
        public string Text { get; set; }

        /// <summary>
        /// TODO: should be replaces with TextSpan
        /// </summary>
        [Key(UstFieldOffset + 1)]
        public int EscapeCharsLength { get; set; } = 1;

        [IgnoreMember]
        public override string TextValue => Text ?? Substring;

        [IgnoreMember]
        public TextSpan ViewTextSpan => EscapeCharsLength == 0
            ? TextSpan
            : new TextSpan(TextSpan.Start - EscapeCharsLength, TextSpan.Length + 2 * EscapeCharsLength, TextSpan.File);

        public StringLiteral(string text, TextSpan textSpan, int escapeCharsLength = 1)
            : base(textSpan)
        {
            Text = text;
            EscapeCharsLength = escapeCharsLength;
        }

        public StringLiteral(TextSpan textSpan, RootUst rootUst, int escapeCharsLength = 1)
            : base(textSpan)
        {
            Root = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
            EscapeCharsLength = escapeCharsLength;
        }

        public StringLiteral(string text)
        {
            Text = text;
        }

        public StringLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            return String.Compare(TextValue, ((StringLiteral)other).TextValue, StringComparison.Ordinal);
        }

        public override string ToString() => $"\"{TextValue}\"";
    }
}