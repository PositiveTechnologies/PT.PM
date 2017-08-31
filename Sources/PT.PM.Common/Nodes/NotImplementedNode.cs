namespace PT.PM.Common.Nodes
{
    public class NotImplementedNode : UstNode
    {
        public override NodeType NodeType => NodeType.NotImplementedNode;

        public NotImplementedNode(TextSpan textSpan, RootNode rootNode)
            : base(textSpan, rootNode)
        {
            TextSpan = textSpan;
            Root = rootNode;
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
