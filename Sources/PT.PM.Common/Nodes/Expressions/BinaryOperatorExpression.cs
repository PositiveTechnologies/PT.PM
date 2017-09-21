using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class BinaryOperatorExpression : Expression
    {
        public override UstKind Kind => UstKind.BinaryOperatorExpression;

        public Expression Left { get; set; }

        public BinaryOperatorLiteral Operator { get; set; }

        public Expression Right { get; set; }

        public BinaryOperatorExpression(Expression left, BinaryOperatorLiteral op, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public BinaryOperatorExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Left, Operator, Right };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Left, Operator, Right };
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }
    }
}
