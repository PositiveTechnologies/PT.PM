namespace PT.PM.Common.Nodes.Tokens
{
    public class CommentLiteral : Token
    {
        public override NodeType NodeType => NodeType.CommentLiteral;

        public virtual string Comment { get; set; }

        public override string TextValue { get { return Comment; } }

        public CommentLiteral(string comment, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Comment = comment;
        }

        public CommentLiteral(string comment, TextSpan textSpan)
           : base(textSpan)
        {
            Comment = comment;
        }

        public CommentLiteral()
        {
        }
    }
}
