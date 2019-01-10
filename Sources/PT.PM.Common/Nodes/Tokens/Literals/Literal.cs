using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    [Union((int)NodeType.BinaryOperatorLiteral, typeof(BinaryOperatorLiteral))]
    [Union((int)NodeType.BooleanLiteral, typeof(BooleanLiteral))]
    [Union((int)NodeType.CommentLiteral, typeof(CommentLiteral))]
    [Union((int)NodeType.FloatLiteral, typeof(FloatLiteral))]
    [Union((int)NodeType.IntLiteral, typeof(IntLiteral))]
    [Union((int)NodeType.ModifierLiteral, typeof(ModifierLiteral))]
    [Union((int)NodeType.NullLiteral, typeof(NullLiteral))]
    [Union((int)NodeType.StringLiteral, typeof(StringLiteral))]
    [Union((int)NodeType.UnaryOperatorLiteral, typeof(UnaryOperatorLiteral))]
    public abstract class Literal : Token
    {
        protected Literal(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Literal()
        {
        }
    }
}
