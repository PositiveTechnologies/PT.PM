using System;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class UnaryOperatorExpression : Expression
    {
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

        public override Ust[] GetChildren()
        {
            return new Ust[] { Operator, Expression };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Operator, Expression };
        }

        public override string ToString()
        {
            UnaryOperator op = Operator.UnaryOperator;
            if (UnaryOperatorLiteral.PrefixTextUnaryOperator.ContainsValue(op))
            {
                UnaryOperator unaryOp = Operator.UnaryOperator;
                string spaceOrEmpty = unaryOp == UnaryOperator.Delete || unaryOp == UnaryOperator.TypeOf ||
                    unaryOp == UnaryOperator.Await || unaryOp == UnaryOperator.Void
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
    }
}
