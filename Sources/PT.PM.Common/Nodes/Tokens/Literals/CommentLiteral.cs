using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class CommentLiteral : Ust
    {
        public CommentLiteral(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public CommentLiteral()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}
