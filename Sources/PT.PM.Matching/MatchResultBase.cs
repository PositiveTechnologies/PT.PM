using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public abstract class MatchResultBase<TPattern> : IMatchResultBase
    {
        public abstract string PatternKey { get; }

        public abstract TextFile SourceFile { get; }

        public bool Suppressed { get; protected set; }

        public TPattern Pattern { get; set; }

        public TextSpan[] TextSpans { get; set; } = ArrayUtils<TextSpan>.EmptyArray;
    }
}
