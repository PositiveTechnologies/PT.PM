using PT.PM.Common;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public interface IMatchResultBase
    {
        string PatternKey { get; }

        bool Suppressed { get; }
    }

    public abstract class MatchResultBase<TPattern>
    {
        public CodeFile SourceCodeFile { get; set; }

        public TPattern Pattern { get; set; }

        public TextSpan[] TextSpans { get; set; } = ArrayUtils<TextSpan>.EmptyArray;
    }
}
