using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes;

namespace PT.PM.Patterns.Nodes
{
    public class PatternExpressionInsideExpression : PatternExpression
    {
        public override NodeType NodeType => NodeType.PatternExpressionInsideExpression;

        public PatternExpressionInsideExpression()
        {
        }

        public PatternExpressionInsideExpression(Expression expression = null, bool not = false)
            : base(expression, not)
        {
        }

        protected override int Compare(UstNode other)
        {
            return other.DoesAnyDescendantMatchPredicate(astNode => Expression.Equals(astNode)) ? 0 : 1;
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Expression };
        }

        public override string ToString()
        {
            if (Expression == null)
            {
                return "#*";
            }

            return (Not ? "<[~]>" : "") + "#* " + Expression.ToString() + " #*";
        }
    }
}
