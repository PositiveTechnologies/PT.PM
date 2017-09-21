using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnd : Expression
    {
        public override UstKind Kind => UstKind.PatternAnd;

        public List<Expression> Expressions { get; set; }

        public PatternAnd(List<Expression> expressions, TextSpan textSpan) :
            base(textSpan)
        {
            Expressions = expressions ?? throw new ArgumentNullException("expressions should be not null");
            if (Expressions.Count < 2)
            {
                throw new ArgumentException("expressions size should be greater than or equal to 2 ");
            }
        }

        public PatternAnd()
        {
            Expressions = new List<Expression>();
        }

        public override Ust[] GetChildren()
        {
            return Expressions.ToArray();
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternAnd)
            {
                var otherPatternAnd = (PatternAnd)other;
                return GetChildren().CompareTo(otherPatternAnd.GetChildren());
            }

            foreach (var expression in Expressions)
            {
                int compareRes = expression.CompareTo(other);
                if (compareRes != 0)
                {
                    return compareRes;
                }
            }

            return 0;
        }

        public override Expression[] GetArgs()
        {
            return Expressions.ToArray();
        }
    }
}
