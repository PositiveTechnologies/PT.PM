using PT.PM.Common.Nodes;
using PT.PM.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Patterns.Nodes
{
    public class PatternComment : CommentLiteral
    {
        public override NodeType NodeType => NodeType.PatternComment;

        [JsonIgnore]
        public int Offset { get; private set; }

        [JsonIgnore]
        public int Length { get; private set; }

        public Regex Regex { get; set; }

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

        public PatternComment(string comment, TextSpan textSpan, FileNode fileNode)
            : base(comment, textSpan, fileNode)
        {
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
            
            var match = Regex.Match(((CommentLiteral)other).Comment);
            if (match.Success)
            {
                Offset = match.Index;
                Length = match.Length;
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public override string ToString() => $"Comment: \"{Comment}\"";
    }
}
