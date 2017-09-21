namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class NullLiteral : Literal
    {
        public override UstKind Kind => UstKind.NullLiteral;

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
