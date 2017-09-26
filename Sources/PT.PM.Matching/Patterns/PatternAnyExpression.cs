using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnyExpression : PatternBase
    {
        public PatternAnyExpression()
        {
        }

        public PatternAnyExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => "#";

        public override bool Match(Ust ust, MatchingContext context)
        {
            return ust is Expression;
        }
    }
}
