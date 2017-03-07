namespace PT.PM.Common.Nodes.Tokens
{
    public class NullLiteral : Token
    {
        public override NodeType NodeType => NodeType.NullLiteral;

        public NullLiteral(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public NullLiteral()
        {
        }

        public override string TextValue
        {
            get { return "null"; }
        }
    }
}
