using System.Globalization;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class FloatLiteral : Literal
    {
        [Key(0)] public override UstType UstType => UstType.FloatLiteral;

        [Key(UstFieldOffset)]
        public double Value { get; set; }

        [IgnoreMember]
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
