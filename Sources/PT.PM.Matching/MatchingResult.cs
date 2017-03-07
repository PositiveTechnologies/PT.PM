using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class MatchingResult
    {
        public Pattern Pattern { get; set; }

        public IList<UstNode> Nodes { get; set; }

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
            Nodes = ArrayUtils<UstNode>.EmptyArray;
        }

        public MatchingResult(Pattern pattern, UstNode node)
            : this(pattern, new[] { node })
        {
        }

        public MatchingResult(Pattern pattern, IList<UstNode> nodes)
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
