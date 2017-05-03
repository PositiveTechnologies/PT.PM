using PT.PM.Common;

namespace PT.PM.Patterns.Nodes
{
    public interface IAbsoluteLocationMatching
    {
        TextSpan MatchedLocation { get; set; }
    }
}
