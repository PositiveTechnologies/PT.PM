using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternExpressionInsideNode : PatternBase
    {
        public PatternBase Expression { get; set; }

        public PatternExpressionInsideNode()
        {
        }

        public PatternExpressionInsideNode(PatternBase expression, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = expression;
        }

        public override Ust[] GetChildren() => new Ust[] { Expression };

        public override string ToString()
        {
            if (Expression == null)
            {
                return "#*";
            }

            return "<{ " + Expression.ToString() + " }>";
        }

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return false;
            }

            return ust.DoesAnyDescendantMatchPredicate(ustNode => MatchExpression(ustNode, context));
        }

        protected bool MatchExpression(Ust other, MatchingContext context)
        {
            if (Expression == null)
            {
                if (other == null)
                {
                    return true;
                }
                return false;
            }
            return Expression.Match(other, context);
        }
    }
}
