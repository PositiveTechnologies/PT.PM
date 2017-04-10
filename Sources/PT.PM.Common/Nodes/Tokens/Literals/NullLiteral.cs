namespace PT.PM.Common.Nodes.Tokens.Literals
{
    public class NullLiteral : Literal
    {
        public override NodeType NodeType => NodeType.NullLiteral;

        public NullLiteral(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public NullLiteral()
        {
        }

        public override string TextValue => "null";
    }
}
