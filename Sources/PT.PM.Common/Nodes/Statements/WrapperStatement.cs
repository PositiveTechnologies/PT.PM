namespace PT.PM.Common.Nodes.Statements
{
    public class WrapperStatement : Statement
    {
        public override NodeType NodeType => NodeType.WrapperStatement;

        public UstNode Node { get; set; }

        public WrapperStatement(UstNode node)
            : base(node.TextSpan)
        {
            Node = node;
        }

        public WrapperStatement(UstNode node, TextSpan textSpan)
            : base(textSpan)
        {
            Node = node;
        }

        public WrapperStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Node };
        }
    }
}
