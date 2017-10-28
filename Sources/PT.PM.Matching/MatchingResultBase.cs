using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching
{
    public interface IMatchingResultBase
    {
    }

    public abstract class MatchingResultBase<TPattern>
    {
        public RootUst RootUst { get; set; }

        public TPattern Pattern { get; set; }

        public TextSpan[] TextSpans { get; set; } = ArrayUtils<TextSpan>.EmptyArray;
    }
}
