namespace PT.PM.Common.Nodes.Expressions
{
    public class WrapperExpression : Expression
    {
        public override UstKind Kind => UstKind.WrapperExpression;

        public Ust Node { get; set; }

        public WrapperExpression(Ust node, TextSpan textSpan)
            : base(textSpan)
        {
            Node = node;
        }

        public WrapperExpression(Ust node)
            : base(node.TextSpan)
        {
            Node = node;
        }

        public WrapperExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Node };
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
