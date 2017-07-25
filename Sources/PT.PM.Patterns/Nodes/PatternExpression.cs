using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Patterns.Nodes
{
    public class PatternExpression : Expression
    {
        public override NodeType NodeType => NodeType.PatternExpression;

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

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            int compareResult;
            if (other.NodeType == NodeType.PatternExpression)
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
                return Not ? (int)NodeType : 0;
            }

            compareResult = Compare(other);
            if (Not)
            {
                compareResult = compareResult != 0 ? 0 : 1;
            }

            return compareResult;
        }

        protected virtual int Compare(UstNode other)
        {
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
                return "#";
            }

            return (Not ? "<~>" : "") + Expression.ToString();
        }
    }
}
