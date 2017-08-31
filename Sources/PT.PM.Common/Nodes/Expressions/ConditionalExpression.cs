namespace PT.PM.Common.Nodes.Expressions
{
    public class ConditionalExpression : Expression
    {
        public override NodeType NodeType => NodeType.ConditionalExpression;

        public Expression Condition { get; set; }

        public Expression TrueExpression { get; set; }

        public Expression FalseExpression { get; set; }

        public ConditionalExpression(Expression condition, Expression trueExpression, Expression falseExpression,
            TextSpan textSpan)
            : base(textSpan)
        {
            Condition = condition;
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;
        }

        public ConditionalExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Condition, TrueExpression, FalseExpression};
        }

        public override string ToString()
        {
            return $"{Condition} ? {TrueExpression} : {FalseExpression}";
        }
    }
}
