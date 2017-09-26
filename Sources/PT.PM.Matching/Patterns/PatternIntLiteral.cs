using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntLiteral : PatternBase
    {
        public long Value { get; set; }

        public PatternIntLiteral()
        {
        }

        public PatternIntLiteral(long value, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Value = value;
        }

        public override Ust[] GetChildren() => ArrayUtils<Expression>.EmptyArray;

        public override string ToString() => Value.ToString();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.IntLiteral)
            {
                return context.Fail();
            }

            long otherValue = ((IntLiteral)ust).Value;
            if (otherValue.Equals(Value))
            {
                return context.AddLocation(ust.TextSpan);
            }

            return context.Fail();
        }
    }
}
