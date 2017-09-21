using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes;
using PT.PM.Common;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternExpressionInsideNode : Expression
    {
        public override UstKind Kind => UstKind.PatternExpressionInsideNode;

        public Expression Expression { get; set; }

        public PatternExpressionInsideNode()
        {
        }

        public PatternExpressionInsideNode(Expression expression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternExpressionInsideNode)
            {
                return CompareExpression(((PatternExpressionInsideNode)other).Expression);
            }

            return other.DoesAnyDescendantMatchPredicate(ustNode => CompareExpression(ustNode) == 0) ? 0 : -1;
        }

        protected int CompareExpression(Ust other)
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

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression };
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
