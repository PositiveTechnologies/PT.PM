namespace PT.PM.Common.Nodes.Statements
{
    public class WrapperStatement : Statement
    {
        public override NodeType NodeType => NodeType.WrapperStatement;

        public UstNode Node { get; set; }

        public WrapperStatement(UstNode node)
            : base(node.TextSpan, node.FileNode)
        {
            Node = node;
        }

        public WrapperStatement(UstNode node, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
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
