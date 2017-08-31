namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class CommentLiteral : Literal
    {
        public override NodeType NodeType => NodeType.CommentLiteral;

        public virtual string Comment { get; set; }

        public override string TextValue => Comment;

        public CommentLiteral(string comment, TextSpan textSpan, RootNode fileNode)
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
