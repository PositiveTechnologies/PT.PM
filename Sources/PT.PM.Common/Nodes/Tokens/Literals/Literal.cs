using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
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
