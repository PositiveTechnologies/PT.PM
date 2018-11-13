using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceToken : PatternUst, ITerminalPattern
    {
        public PatternBaseReferenceToken()
        {
        }

        public PatternBaseReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var baseReferenceToken = ust as BaseReferenceToken;
            if (baseReferenceToken == null)
            {
                return context.Fail();
            }
            
            return context.AddMatch(ust);
        }
    }
}
