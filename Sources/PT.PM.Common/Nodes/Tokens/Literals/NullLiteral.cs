namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class NullLiteral : Literal
    {
        public NullLiteral(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public NullLiteral()
        {
        }

        public override string TextValue => "null";
    }
}
