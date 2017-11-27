using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceToken : PatternUst<BaseReferenceToken>
    {
        public PatternBaseReferenceToken()
        {
        }

        public PatternBaseReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        public override MatchingContext Match(BaseReferenceToken ust, MatchingContext context)
        {
            return context.AddMatch(ust);
        }
    }
}
