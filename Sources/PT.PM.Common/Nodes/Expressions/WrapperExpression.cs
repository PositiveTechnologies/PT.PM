namespace PT.PM.Common.Nodes.Expressions
{
    public class WrapperExpression : Expression
    {
        public override NodeType NodeType => NodeType.WrapperExpression;

        public UstNode Node { get; set; }

        public WrapperExpression(UstNode node, TextSpan textSpan)
            : base(textSpan)
        {
            Node = node;
        }

        public WrapperExpression(UstNode node)
            : base(node.TextSpan)
        {
            Node = node;
        }

        public WrapperExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Node };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[0];
        }

        public override string ToString()
        {
            return $"WrapExpr({Node})";
        }
    }
}
