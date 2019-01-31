using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class StringLiteral : Literal
    {
        [Key(UstFieldOffset)]
        public virtual string Text { get; set; } = "";
        
        [Key(UstFieldOffset + 1)]
        public int EscapeCharsLength { get; set; } = 1;
        
        [IgnoreMember]
        public override string TextValue => Text;

        public StringLiteral(string text, TextSpan textSpan)
            : base(textSpan)
        {
            Text = text;
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

            return String.Compare(Text, ((StringLiteral)other).Text, StringComparison.Ordinal);
        }

        public override string ToString() => $"\"{Text}\"";
    }
}