using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceToken : PatternUst
    {
        public PatternBaseReferenceToken()
        {
        }

        public PatternBaseReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (!(ust is BaseReferenceToken))
            {
                return context.Fail();
            }

            return context.AddMatch(ust);
        }
    }
}
