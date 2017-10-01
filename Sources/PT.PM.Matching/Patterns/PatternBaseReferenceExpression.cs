using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceExpression : PatternBase
    {
        public PatternBaseReferenceExpression()
        {
        }

        public PatternBaseReferenceExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (!(ust is BaseReferenceExpression))
            {
                return context.Fail();
            }

            return context.AddMatch(ust);
        }
    }
}
