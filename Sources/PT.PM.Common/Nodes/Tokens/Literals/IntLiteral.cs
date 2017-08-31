namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class IntLiteral : Literal
    {
        public override NodeType NodeType => NodeType.IntLiteral;

        public long Value { get; set; }

        public override string TextValue => Value.ToString();

        public IntLiteral(long value)
            : this(value, default(TextSpan))
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

        public override int CompareTo(UstNode other)
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
