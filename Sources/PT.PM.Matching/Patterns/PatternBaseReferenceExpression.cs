using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternBaseReferenceExpression : PatternBase
    {
        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public PatternBaseReferenceExpression()
        {
        }

        public PatternBaseReferenceExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override string ToString() => "base";

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.BaseReferenceExpression)
            {
                return false;
            }

            return true;
        }
    }
}
