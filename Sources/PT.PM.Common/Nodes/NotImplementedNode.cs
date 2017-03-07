namespace PT.PM.Common.Nodes
{
    public class NotImplementedNode : UstNode
    {
        public override NodeType NodeType => NodeType.NotImplementedNode;

        public NotImplementedNode(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            TextSpan = textSpan;
            FileNode = fileNode;
        }

        public NotImplementedNode()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }
    }
}
