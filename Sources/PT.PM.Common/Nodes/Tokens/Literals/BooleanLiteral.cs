using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class BooleanLiteral : Literal
    {
        [Key(UstFieldOffset)]
        public bool Value { get; set; }

        public BooleanLiteral(bool value)
            : this(value, default(TextSpan))
        {
        }

        public BooleanLiteral(bool value, TextSpan textSpan)
            : base(textSpan)
        {
            Value = value;
        }

        [IgnoreMember]
        public override string TextValue => Value.ToString();
        
        public BooleanLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            var result = Value ^ ((BooleanLiteral)other).Value;
            if (result != false)
            {
                return Value ? 1 : -1;
            }

            return 0;
        }
    }
}
