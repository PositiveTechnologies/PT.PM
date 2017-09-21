using System;
using Newtonsoft.Json;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common;

namespace PT.PM.Matching.Patterns
{
    public class PatternExpression : Expression
    {
        public override UstKind Kind => UstKind.PatternExpression;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Expression Expression { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Not { get; set; }

        public PatternExpression(Expression expression = null, bool not = false)
        {
            Expression = expression;
            TextSpan = expression.TextSpan;
            Not = not;
        }

        public PatternExpression()
        {
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            int compareResult;
            if (other.Kind == UstKind.PatternExpression)
            {
                var otherPatternExpression = (PatternExpression)other;

                if (Expression == null ^ otherPatternExpression.Expression == null)
                {
                    return 1;
                }
                if (Expression == null)
                {
                    if (otherPatternExpression.Expression == null)
                    {
                        return Not == otherPatternExpression.Not ? 0 : 1;
                    }
                    else
                    {
                        return 1;
                    }
                }

                compareResult = Expression.CompareTo(otherPatternExpression.Expression);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                return Not == otherPatternExpression.Not ? 0 : 1;
            }

            if (!(other is Expression)) // compare only with expressions.
            {
                return -1;
            }

            if (Expression == null)     // Any expression.
            {
                return Not ? (int)Kind : 0;
            }

            compareResult = Compare(other);
            if (Not)
            {
                compareResult = compareResult != 0 ? 0 : 1;
            }

            return compareResult;
        }

        protected virtual int Compare(Ust other)
        {
            return Expression.CompareTo(other);
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression };
        }

        public override Expression[] GetArgs()
        {
            if (Expression == null)
            {
                return ArrayUtils<Expression>.EmptyArray;
            }

            return Expression.GetArgs();
        }

        public override string ToString()
        {
            if (Expression == null)
            {
                return "#";
            }

            return (Not ? "<~>" : "") + Expression.ToString();
        }
    }
}
