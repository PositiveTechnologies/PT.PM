using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class UnaryOperatorExpression : Expression
    {
        public override NodeType NodeType => NodeType.UnaryOperatorExpression;

        public UnaryOperatorLiteral Operator { get; set; }

        public Expression Expression { get; set; }

        public UnaryOperatorExpression(UnaryOperatorLiteral op, Expression ex, TextSpan textSpan)
            : base(textSpan)
        {
            Operator = op;
            Expression = ex;
        }

        public UnaryOperatorExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Operator, Expression};
        }

        public override string ToString()
        {
            UnaryOperator op = Operator.UnaryOperator;
            if (UnaryOperatorLiteral.PrefixTextUnaryOperator.ContainsValue(op))
            {
                return $"{Operator}{Expression}";
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
    }
}
