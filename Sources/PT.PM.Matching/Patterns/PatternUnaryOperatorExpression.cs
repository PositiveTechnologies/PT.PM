using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternUnaryOperatorExpression : PatternUst<UnaryOperatorExpression>
    {
        public PatternUst Expression { get; set; }

        public PatternUnaryOperatorLiteral Operator { get; set; }

        public override string ToString()
        {
            UnaryOperator op = Operator.UnaryOperator;
            if (UnaryOperatorLiteral.PrefixTextUnaryOperator.ContainsValue(op))
            {
                string spaceOrEmpty = op == UnaryOperator.Delete || op == UnaryOperator.TypeOf ||
                                      op == UnaryOperator.Await || op == UnaryOperator.Void
                    ? " "
                    : "";
                return $"{Operator}{spaceOrEmpty}{Expression}";
            }
            else if (UnaryOperatorLiteral.PostfixTextUnaryOperator.ContainsValue(op))
            {
                return $"{Expression}{Operator}";
            }
            else
            {
                return $"{Operator} {Expression}";
            }
        }

        public override MatchContext Match(UnaryOperatorExpression ust, MatchContext context)
        {
            MatchContext newContext = Expression.MatchUst(ust.Expression, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Operator.Match(ust.Operator, newContext);

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
