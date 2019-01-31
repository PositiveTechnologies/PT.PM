using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class CommentLiteral : Literal
    {
        [Key(UstFieldOffset)]
        public virtual string Comment { get; set; }

        [IgnoreMember]
        public override string TextValue => Comment;

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
