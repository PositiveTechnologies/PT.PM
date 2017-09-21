namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class BooleanLiteral : Literal
    {
        public override UstKind Kind => UstKind.BooleanLiteral;

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

        public BooleanLiteral()
        {
        }

        public override string TextValue => Value.ToString();

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
