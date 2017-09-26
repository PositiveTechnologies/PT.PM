using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System.Collections.Generic;
using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<PatternRootUst>
    {
        public SourceCodeFile SourceCodeFile => RootUst.SourceCodeFile;

        public TextSpan TextSpan => TextSpans.FirstOrDefault();

        public MatchingResult()
        {
        }

        public MatchingResult(RootUst rootUst, PatternRootUst pattern, IEnumerable<TextSpan> textSpans)
        {
            RootUst = rootUst;
            Pattern = pattern;
            textSpans = textSpans.ToArray();
        }

        public override string ToString()
        {
            return $"Pattern {Pattern} mathched at {(string.Join(", ", TextSpans))}";
        }
    }
}
