using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes;
using PT.PM.Common;
using System;

namespace PT.PM.Patterns.Nodes
{
    public class PatternExpressionInsideNode : Expression
    {
        public override NodeType NodeType => NodeType.PatternExpressionInsideNode;

        public Expression Expression { get; set; }

        public PatternExpressionInsideNode()
        {
        }

        public PatternExpressionInsideNode(Expression expression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternExpressionInsideNode)
            {
                return CompareExpression(((PatternExpressionInsideNode)other).Expression);
            }

            return other.DoesAnyDescendantMatchPredicate(ustNode => CompareExpression(ustNode) == 0) ? 0 : -1;
        }

        protected int CompareExpression(UstNode other)
        {
            if (Expression == null)
            {
                if (other == null)
                {
                    return 0;
                }
                return -other.CompareTo(null);
            }
            return Expression.CompareTo(other);
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

            return "#* " + Expression.ToString() + " #*";
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Expression };
        }
    }
}
