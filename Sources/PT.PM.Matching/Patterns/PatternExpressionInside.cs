using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternArbitraryDepthExpression : PatternBase
    {
        public PatternBase Expression { get; set; }

        public PatternArbitraryDepthExpression()
        {
        }

        public PatternArbitraryDepthExpression(PatternBase expression, TextSpan textSpan = default(TextSpan))
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            var result = ust.DoesAnyDescendantMatchPredicate(ustNode => MatchExpression(ustNode, context).Success);
            return context.Change(result).AddMatchIfSuccess(ust);
        }

        protected MatchingContext MatchExpression(Ust other, MatchingContext context)
        {
            if (Expression == null)
            {
                if (other == null)
                {
                    return context;
                }
                return context.Fail();
            }
            return Expression.Match(other, context);
        }
    }
}
