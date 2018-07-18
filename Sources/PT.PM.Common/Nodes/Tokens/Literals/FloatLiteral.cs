using System.Globalization;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class FloatLiteral : Literal
    {
        public double Value { get; set; }

        public override string TextValue => Value.ToString(CultureInfo.InvariantCulture);

        public FloatLiteral(double value)
            : this(value, default)
        {
        }

        public FloatLiteral(double value, TextSpan textSpan)
            : base(textSpan)
        {
            Value = value;
        }

        public FloatLiteral()
        {
        }

        public override int CompareTo(Ust other)
        {
            var baseCompareResult = base.CompareTo(other);
            if (baseCompareResult != 0)
            {
                return baseCompareResult;
            }

            return Value.CompareTo(((FloatLiteral)other).Value);
        }
    }
}
