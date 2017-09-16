using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternNot : Expression
    {
        public override NodeType NodeType => NodeType.PatternNot;

        public Expression Expression { get; set; }

        public PatternNot(Expression expression, TextSpan textSpan) :
            base(textSpan)
        {
            Expression = expression;
        }

        public PatternNot()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Expression };
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternNot)
            {
                var otherPatternNot = (PatternNot)other;
                return Expression.CompareTo(otherPatternNot.Expression);
            }

            int compareRes = Expression.CompareTo(other);
            return compareRes == 0 ? -1 : 0;
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Expression };
        }
    }
}
