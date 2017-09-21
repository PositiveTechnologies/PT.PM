using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System.Collections.Generic;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<PatternRootUst>
    {
        public SourceCodeFile SourceCodeFile => Nodes.FirstOrDefault()?.Root?.SourceCodeFile;

        public TextSpan TextSpan { get; private set; }

        public MatchingResult()
        {
            Nodes = new List<Ust>();
        }

        public MatchingResult(PatternRootUst pattern, Ust node, TextSpan textSpan)
            : this(pattern, new List<Ust> { node })
        {
            TextSpan = textSpan;
        }

        public MatchingResult(PatternRootUst pattern, List<Ust> nodes)
        {
            Pattern = pattern;
            Nodes = nodes;
        }

        public override string ToString()
        {
            string textSpan = Nodes.Count == 1 ? TextSpan.ToString() : string.Join("; ", Nodes.Select(node => node.TextSpan));
            return $"Pattern {Pattern} mathched at {textSpan}";
        }
    }
}
