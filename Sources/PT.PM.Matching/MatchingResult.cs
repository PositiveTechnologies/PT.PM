using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using System.Collections.Generic;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<PatternRootNode>
    {
        public SourceCodeFile SourceCodeFile => Nodes.FirstOrDefault()?.Root?.SourceCodeFile;

        public TextSpan TextSpan { get; private set; }

        public MatchingResult()
        {
            Nodes = new List<UstNode>();
        }

        public MatchingResult(PatternRootNode pattern, UstNode node, TextSpan textSpan)
            : this(pattern, new List<UstNode> { node })
        {
            TextSpan = textSpan;
        }

        public MatchingResult(PatternRootNode pattern, List<UstNode> nodes)
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
