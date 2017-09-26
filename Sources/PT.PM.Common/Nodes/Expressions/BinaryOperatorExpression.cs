using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class BinaryOperatorExpression : Expression
    {
        public override UstKind Kind => UstKind.BinaryOperatorExpression;

        public Expression Left { get; set; }

        public BinaryOperatorLiteral Operator { get; set; }

        public Expression Right { get; set; }

        public BinaryOperatorExpression()
        {
        }

        public BinaryOperatorExpression(Expression left, BinaryOperatorLiteral op, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override Ust[] GetChildren() => new Ust[] { Left, Operator, Right };

        public override Expression[] GetArgs() => new Expression[] { Left, Operator, Right };

        public override string ToString() => $"{Left} {Operator} {Right}";
    }
}
