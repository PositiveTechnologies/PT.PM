using PT.PM.Common.Nodes;
using PT.PM.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Patterns.Nodes
{
    public class PatternComment : CommentLiteral, IRelativeLocationMatching
    {
        public override NodeType NodeType => NodeType.PatternComment;

        [JsonIgnore]
        public Regex Regex { get; set; }

        public TextSpan[] MatchedLocations { get; set; }

        public override string Comment
        {
            get
            {
                return Regex.ToString();
            }
            set
            {
                Regex = new Regex(value, RegexOptions.Compiled);
            }
        }

        public PatternComment(string comment, TextSpan textSpan)
            : base(comment, textSpan)
        {
        }

        public PatternComment()
        {
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternComment)
            {
                return Comment.CompareTo(((PatternComment)other).Comment);
            }

            if (other.NodeType != NodeType.CommentLiteral)
            {
                return NodeType - other.NodeType;
            }

            MatchedLocations = PatternHelper.MatchRegex(Regex, ((CommentLiteral)other).Comment);
            return MatchedLocations.Length == 0 ? 1 : 0;
        }

        public override string ToString() => $"Comment: \"{Comment}\"";
    }
}
