using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternThisReferenceToken : PatternUst, ITerminalPattern
    {

        public PatternThisReferenceToken()
        {
        }

        public PatternThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "this";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            return ust is ThisReferenceToken thisReferenceToken
                ? context.AddMatch(thisReferenceToken)
                : context.Fail();
        }
    }
}
