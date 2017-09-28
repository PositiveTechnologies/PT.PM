using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public abstract class PatternBase : Ust
    {
        protected PatternBase(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public abstract MatchingContext Match(Ust ust, MatchingContext context);
    }
}
