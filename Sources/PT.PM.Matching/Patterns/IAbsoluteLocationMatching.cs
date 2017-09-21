using PT.PM.Common;

namespace PT.PM.Matching.Patterns
{
    public interface IAbsoluteLocationMatching
    {
        TextSpan MatchedLocation { get; set; }
    }
}
