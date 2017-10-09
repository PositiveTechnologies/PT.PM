using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternThisReferenceToken : PatternUst
    {

        public PatternThisReferenceToken()
        {
        }

        public PatternThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "this";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (!(ust is ThisReferenceToken))
            {
                return context.Fail();
            }

            return context.AddMatch(ust);
        }
    }
}
