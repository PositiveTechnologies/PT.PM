using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternNot : Expression
    {
        public override UstKind Kind => UstKind.PatternNot;

        public Expression Expression { get; set; }

        public PatternNot(Expression expression, TextSpan textSpan) :
            base(textSpan)
        {
            Expression = expression;
        }

        public PatternNot()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression };
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternNot)
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
