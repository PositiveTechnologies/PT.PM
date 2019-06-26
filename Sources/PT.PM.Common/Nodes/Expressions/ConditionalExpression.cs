using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class ConditionalExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.ConditionalExpression;

        [Key(UstFieldOffset)]
        public Expression Condition { get; set; }

        [Key(UstFieldOffset + 1)]
        public Expression TrueExpression { get; set; }

        [Key(UstFieldOffset + 2)]
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

        public override Ust[] GetChildren()
        {
            return new Ust[] { Condition, TrueExpression, FalseExpression };
        }

        public override Expression[] GetArgs() => new[] { Condition };

        public override string ToString()
        {
            return $"{Condition} ? {TrueExpression.ToStringWithTrailSpace()}: {FalseExpression}";
        }
    }
}
