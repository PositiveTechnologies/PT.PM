using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using System.Collections.Generic;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<Pattern>
    {
        public FileNode FileNode => Nodes.FirstOrDefault()?.FileNode;

        public TextSpan TextSpan { get; private set; }

        public MatchingResult()
        {
            Nodes = new List<UstNode>();
        }

        public MatchingResult(Pattern pattern, UstNode node, TextSpan textSpan)
            : this(pattern, new List<UstNode> { node })
        {
            TextSpan = textSpan;
        }

        public MatchingResult(Pattern pattern, List<UstNode> nodes)
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
