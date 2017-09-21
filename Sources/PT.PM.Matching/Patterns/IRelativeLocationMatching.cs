using PT.PM.Common;

namespace PT.PM.Matching.Patterns
{
    public interface IRelativeLocationMatching
    {
        TextSpan[] MatchedLocations { get; set; }
    }
}
