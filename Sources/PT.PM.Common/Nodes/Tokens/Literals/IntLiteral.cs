using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class IntLiteral : Literal
    {
        [Key(UstFieldOffset)]
        public long Value { get; set; }

        [IgnoreMember]
        public override string TextValue => Value.ToString();

        public IntLiteral(long value)
            : this(value, default)
        {
        }

        public IntLiteral(long value, TextSpan textSpan)
            : base(textSpan)
        {
            Value = value;
        }

        public IntLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            return Value.CompareTo(((IntLiteral)other).Value);
        }
    }
}
