using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Patterns.Nodes
{
    public class PatternIntLiteral : IntLiteral
    {
        public override NodeType NodeType => NodeType.PatternIntLiteral;

        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public PatternIntLiteral()
            : this(long.MinValue, long.MaxValue)
        {
        }

        public PatternIntLiteral(long value)
            : this(value, value)
        {
        }

        public PatternIntLiteral(long minValue, long maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternIntLiteral)
            {
                var otherPatternIntLiteral = (PatternIntLiteral)other;
                return MinValue == otherPatternIntLiteral.MinValue && MaxValue == otherPatternIntLiteral.MaxValue ? 0 : 1;
            }

            if (other.NodeType != NodeType.IntLiteral)
            {
                return NodeType - other.NodeType;
            }

            long otherValue = ((IntLiteral)other).Value;
            int result;
            if (otherValue < MinValue)
            {
                result = 1;
            }
            else if (otherValue >= MaxValue)
            {
                result = -1;
            }
            else
            {
                result = 0;
            }

            return result;
        }

        public override string TextValue
        {
            get
            {
                if (MinValue == MaxValue)
                {
                    return MinValue.ToString();
                }

                return $"{(MinValue == long.MinValue ? "-(∞" : "[" + MinValue.ToString())}"
                      + ".."
                      + $"{(MaxValue == long.MaxValue ? "∞)" : MaxValue.ToString() + ")")}";
            }
        }
    }
}
