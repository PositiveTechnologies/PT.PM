using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntRangeLiteral : PatternBase
    {
        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public PatternIntRangeLiteral()
            : this(long.MinValue, long.MaxValue)
        {
        }

        public PatternIntRangeLiteral(long value, TextSpan textSpan = default(TextSpan))
            : this(value, value, textSpan)
        {
        }

        public PatternIntRangeLiteral(long minValue, long maxValue, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override Ust[] GetChildren() => ArrayUtils<Expression>.EmptyArray;

        public override string ToString()
        {
            if (MinValue == MaxValue)
            {
                return MinValue.ToString();
            }

            string lowerBound = MinValue == long.MinValue ? "" : MinValue.ToString();
            string upperBound = MaxValue == long.MaxValue ? "" : MaxValue.ToString();
            return $"<({lowerBound}..{upperBound})>";
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IntLiteral intLiteral && 
                (intLiteral.Value >= MinValue && intLiteral.Value < MaxValue))
            {
                newContext = context.AddMatch(ust);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
