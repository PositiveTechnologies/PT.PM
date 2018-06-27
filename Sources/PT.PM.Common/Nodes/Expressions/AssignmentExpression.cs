namespace PT.PM.Common.Nodes.Expressions
{
    public class AssignmentExpression : Expression
    {
        public Expression Left { get; set; }

        public Expression Right { get; set; }

        public BinaryOperatorExpression BinaryExpression { get; set; }

        public AssignmentExpression(Expression left, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Right = right;
        }

        public AssignmentExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            if(BinaryExpression != null)
            {
                return new Ust[] { Left, Right, BinaryExpression };
            }
            return new Ust[] { Left, Right };
        }
        

        public override Expression[] GetArgs() => new Expression[] { Left, Right };

        public override string ToString()
        {
            return Right == null
                ? Left == null
                ? " = "
                : Left.ToString()
                : $"{Left} {BinaryExpression?.Operator}= {Right}";
        }
    }
}
