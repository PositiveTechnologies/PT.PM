using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternUst
    {
        MatchingContext Match(Ust ust, MatchingContext context);
    }
}
