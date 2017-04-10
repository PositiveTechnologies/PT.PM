using System.Globalization;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class FloatLiteral : Literal
    {
        public override NodeType NodeType => NodeType.FloatLiteral;

        public double Value { get; set; }

        public override string TextValue => Value.ToString(CultureInfo.InvariantCulture);

        public FloatLiteral(double value)
            : this(value, default(TextSpan), null)
        {
        }

        public FloatLiteral(double value, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Value = value;
        }

        public FloatLiteral()
        {
        }

        public override int CompareTo(UstNode other)
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
