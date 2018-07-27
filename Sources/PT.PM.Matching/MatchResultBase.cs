using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching
{
    public interface IMatchResultBase
    {
        bool Suppressed { get; }
    }

    public abstract class MatchResultBase<TPattern>
    {
        public RootUst RootUst { get; set; }

        public TPattern Pattern { get; set; }

        public TextSpan[] TextSpans { get; set; } = ArrayUtils<TextSpan>.EmptyArray;
    }
}
