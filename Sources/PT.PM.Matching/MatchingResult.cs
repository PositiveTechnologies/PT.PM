using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<PatternRoot>, IMatchingResultBase
    {
        public SourceCodeFile SourceCodeFile => RootUst.SourceCodeFile;

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        public MatchingResult()
        {
        }

        public MatchingResult(RootUst rootUst, PatternRoot pattern, IEnumerable<TextSpan> textSpans)
        {
            RootUst = rootUst;
            Pattern = pattern;
            TextSpans = textSpans.ToArray();
        }

        public override string ToString()
        {
            return $"Pattern {Pattern} mathched at {(string.Join(", ", TextSpans))}";
        }
    }
}
