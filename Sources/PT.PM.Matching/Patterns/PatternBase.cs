using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternBase : Ust, IPatternUst
    {
        public override UstKind Kind => UstKind.Pattern;

        protected PatternBase(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public abstract bool Match(Ust ust, MatchingContext context);
    }
}
