using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class LongLiteral : Literal, INumericLiteral
    {
        [Key(UstFieldOffset)] 
        public long Value { get; set; }

        [IgnoreMember] 
        public override string TextValue => Value.ToString();

        public LongLiteral(long value)
            : this(value, default)
        {
        }

        public LongLiteral(long value, TextSpan textSpan)
            : base(textSpan)
        {
            Value = value;
        }

        public LongLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            return baseCompareResult != 0
                ? baseCompareResult
                : Value.CompareTo(((LongLiteral) other).Value);
        }
    }
}