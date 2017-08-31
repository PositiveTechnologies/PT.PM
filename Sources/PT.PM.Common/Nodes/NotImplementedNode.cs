namespace PT.PM.Common.Nodes
{
    public class NotImplementedNode : UstNode
    {
        public override NodeType NodeType => NodeType.NotImplementedNode;

        public NotImplementedNode(TextSpan textSpan)
            : base(textSpan)
        {
            TextSpan = textSpan;
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
