namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class NullLiteral : Literal
    {
        public override NodeType NodeType => NodeType.NullLiteral;

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
