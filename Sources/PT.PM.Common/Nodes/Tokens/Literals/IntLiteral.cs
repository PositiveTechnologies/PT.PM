using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class IntLiteral : Literal, INumericLiteral
    {
        [Key(UstFieldOffset)]
        public int Value { get; set; }

        [IgnoreMember]
        public override string TextValue => Value.ToString();

        public IntLiteral(int value)
            : this(value, default)
        {
        }

        public IntLiteral(int value, TextSpan textSpan)
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
            return baseCompareResult != 0 
                ? baseCompareResult 
                : Value.CompareTo(((IntLiteral)other).Value);
        }
    }
}
