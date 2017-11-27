using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternNullLiteral : PatternUst<NullLiteral>
    {
        public PatternNullLiteral()
        {
        }

        public PatternNullLiteral(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override string ToString() => "null";

        public override MatchingContext Match(NullLiteral nullLiteral, MatchingContext context)
        {
            return context.AddMatch(nullLiteral);
        }
    }
}
