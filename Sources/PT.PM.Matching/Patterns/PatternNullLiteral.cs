using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternNullLiteral : PatternUst<NullLiteral>, ITerminalPattern
    {
        public PatternNullLiteral()
        {
        }

        public PatternNullLiteral(TextSpan textSpan = default)
            : base(textSpan)
        {
        }

        public override string ToString() => "null";

        public override MatchContext Match(NullLiteral nullLiteral, MatchContext context)
        {
            return context.AddMatch(nullLiteral);
        }
    }
}
