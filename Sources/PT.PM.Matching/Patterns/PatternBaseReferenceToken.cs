using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceToken : PatternUst<BaseReferenceToken>, ITerminalPattern
    {
        public PatternBaseReferenceToken()
        {
        }

        public PatternBaseReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        public override MatchContext Match(BaseReferenceToken ust, MatchContext context)
        {
            return context.AddMatch(ust);
        }
    }
}
