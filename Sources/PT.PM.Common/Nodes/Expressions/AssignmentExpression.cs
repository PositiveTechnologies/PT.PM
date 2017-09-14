namespace PT.PM.Common.Nodes.Expressions
{
    public class AssignmentExpression : Expression
    {
        public override NodeType NodeType => NodeType.AssignmentExpression;

        public Expression Left { get; set; }

        public Expression Right { get; set; }

        public AssignmentExpression(Expression left, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Right = right;
        }

        public AssignmentExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Left, Right };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Left, Right };
        }

        public override string ToString()
        {
            return Right == null ? Left.ToString() : $"{Left} = {Right}";
        }
    }
}
