using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Tokens
{
    public class BaseReferenceExpression : Expression
    {
        public override NodeType NodeType => NodeType.BaseReferenceExpression;

        public BaseReferenceExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public BaseReferenceExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { this };
        }

        public override string ToString()
        {
            return "base";
        }
    }
}
