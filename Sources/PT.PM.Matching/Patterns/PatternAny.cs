using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternBase
    {
        public static PatternAny Instance = new PatternAny();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.MakeSuccess();
            }

            return context.AddMatch(ust);
        }
    }
}
