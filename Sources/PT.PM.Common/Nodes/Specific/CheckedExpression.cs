using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public class CheckedExpression : SpecificExpression
    {
        public override NodeType NodeType => NodeType.CheckedExpression;

        public Expression Expression { get; set; }

        public CheckedExpression(Expression checkedExpression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = checkedExpression;
            TextSpan = textSpan;
        }

        public CheckedExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Expression};
        }
    }
}
