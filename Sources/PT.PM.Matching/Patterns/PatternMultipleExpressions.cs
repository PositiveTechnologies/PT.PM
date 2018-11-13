using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMultipleExpressions : PatternUst
    {
        public PatternMultipleExpressions()
        {
        }

        public PatternMultipleExpressions(TextSpan textSpan = default)
            : base(textSpan)
        {
        }

        public override string ToString() => "#*";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            return ust is Expression expression
                ? context.AddMatch(expression)
                : context.Fail();
        }
    }
}
