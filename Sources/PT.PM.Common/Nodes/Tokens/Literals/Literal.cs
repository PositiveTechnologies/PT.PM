namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public abstract class Literal : Token
    {
        public override NodeType NodeType => NodeType.Literal;

        protected Literal(TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected Literal(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Literal()
        {
        }
    }
}
