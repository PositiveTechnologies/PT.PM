using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class MatchingResult : MatchingResultBase<Pattern>
    {
        public FileNode FileNode => Nodes.FirstOrDefault()?.FileNode;

        public TextSpan TextSpan
        {
            get
            {
                return Nodes.LastOrDefault()?.TextSpan ?? default(TextSpan);
            }
            set
            {
                // TODO: fix
                Nodes = new List<UstNode>() { new Common.Nodes.Tokens.IdToken("temp") { TextSpan = value, FileNode = FileNode } };
            }
        }

        public MatchingResult()
        {
            Nodes = new List<UstNode>();
        }

        public MatchingResult(Pattern pattern, UstNode node)
            : this(pattern, new List<UstNode> { node })
        {
        }

        public MatchingResult(Pattern pattern, List<UstNode> nodes)
        {
            Pattern = pattern;
            Nodes = nodes;
        }

        public override string ToString()
        {
            return $"Pattern {Pattern} mathched at {(string.Join(";", Nodes.Select(node => node.TextSpan)))}";
        }
    }
}
