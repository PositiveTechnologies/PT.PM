using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternUst
    {
        bool Match(Ust ust, MatchingContext context);
    }
}
