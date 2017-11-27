using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternThisReferenceToken : PatternUst<ThisReferenceToken>
    {

        public PatternThisReferenceToken()
        {
        }

        public PatternThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "this";

        public override MatchingContext Match(ThisReferenceToken thisReferenceToken, MatchingContext context)
        {
            return context.AddMatch(thisReferenceToken);
        }
    }
}
