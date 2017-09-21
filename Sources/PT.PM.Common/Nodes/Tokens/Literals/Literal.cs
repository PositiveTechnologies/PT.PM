namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public abstract class Literal : Token
    {
        public override UstKind Kind => UstKind.Literal;

        protected Literal(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Literal()
        {
        }
    }
}
