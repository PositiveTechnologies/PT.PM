using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMultipleExpressions : PatternUst<Expression>
    {
        public PatternMultipleExpressions()
        {
        }

        public PatternMultipleExpressions(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override string ToString() => "#*";

        public override MatchContext Match(Expression expression, MatchContext context)
        {
            return context.AddMatch(expression);
        }
    }
}
