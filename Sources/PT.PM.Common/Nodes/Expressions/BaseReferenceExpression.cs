namespace PT.PM.Common.Nodes.Expressions
{
    public class BaseReferenceExpression : Expression
    {
        public override NodeType NodeType => NodeType.BaseReferenceExpression;

        public BaseReferenceExpression(TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public BaseReferenceExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override string ToString()
        {
            return "base";
        }
    }
}
