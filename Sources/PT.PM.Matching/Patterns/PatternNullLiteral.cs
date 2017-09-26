using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternNullLiteral : PatternBase
    {
        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public PatternNullLiteral()
        {
        }

        public PatternNullLiteral(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override string ToString() => "null";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust is NullLiteral)
            {
                return context;
            }
            else
            {
                return context.Fail();
            }
        }
    }
}
