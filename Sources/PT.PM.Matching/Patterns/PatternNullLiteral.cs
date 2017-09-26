using PT.PM.Common;
using PT.PM.Common.Nodes;

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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.NullLiteral)
            {
                return false;
            }

            return true;
        }
    }
}
