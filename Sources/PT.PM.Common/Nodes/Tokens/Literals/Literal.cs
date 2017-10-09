namespace PT.PM.Common.Nodes.Tokens.Literals
{
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
