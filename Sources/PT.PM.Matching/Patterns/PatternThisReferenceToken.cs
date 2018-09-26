using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternThisReferenceToken : PatternUst<ThisReferenceToken>, ITerminalPattern
    {

        public PatternThisReferenceToken()
        {
        }

        public PatternThisReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "this";

        public override MatchContext Match(ThisReferenceToken thisReferenceToken, MatchContext context)
        {
            return context.AddMatch(thisReferenceToken);
        }
    }
}
