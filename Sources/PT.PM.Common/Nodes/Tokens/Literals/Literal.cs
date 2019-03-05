using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    [Union((int)NodeType.BigIntLiteral, typeof(BigIntLiteral))]
    [Union((int)NodeType.LongLiteral, typeof(LongLiteral))]
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
