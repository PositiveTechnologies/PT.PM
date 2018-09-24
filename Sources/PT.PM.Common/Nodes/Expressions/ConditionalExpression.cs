using System;

namespace PT.PM.Common.Nodes.Expressions
{
    public class ConditionalExpression : Expression
    {
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

        public override Ust[] GetChildren()
        {
            return new Ust[] { Condition, TrueExpression, FalseExpression };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Condition };
        }

        public override string ToString()
        {
            return $"{Condition} ? {TrueExpression.ToStringWithTrailSpace()}: {FalseExpression}";
        }
    }
}
