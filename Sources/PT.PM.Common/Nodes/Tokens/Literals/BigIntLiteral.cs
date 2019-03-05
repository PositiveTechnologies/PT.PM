using System.Numerics;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class BigIntLiteral : Literal, INumericLiteral
    {
        [Key(UstFieldOffset)] 
        public BigInteger Value { get; set; }

        [IgnoreMember] 
        public override string TextValue => Value.ToString();

        public BigIntLiteral(BigInteger value)
            : this(value, default)
        {
        }

        public BigIntLiteral(BigInteger value, TextSpan textSpan)
            : base(textSpan)
        {
            Value = value;
        }

        public BigIntLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            return baseCompareResult != 0
                ? baseCompareResult
                : Value.CompareTo(((BigIntLiteral) other).Value);
        }
    }
}