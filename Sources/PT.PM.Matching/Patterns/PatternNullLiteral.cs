using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternNullLiteral : PatternUst, ITerminalPattern
    {
        public PatternNullLiteral()
        {
        }

        public PatternNullLiteral(TextSpan textSpan = default)
            : base(textSpan)
        {
        }

        public override string ToString() => "null";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            return ust is NullLiteral nullLiteral
                ? context.AddMatch(nullLiteral)
                : context.Fail();
        }
    }
}
