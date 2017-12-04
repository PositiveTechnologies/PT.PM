using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnyExpression : PatternUst<Expression>
    {
        public PatternAnyExpression()
        {
        }

        public PatternAnyExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override string ToString() => "#";

        public override MatchContext Match(Expression expression, MatchContext context)
        {
            return context.AddMatch(expression);
        }
    }
}
