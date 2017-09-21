using System;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public class CheckedExpression : SpecificExpression
    {
        public override UstKind Kind => UstKind.CheckedExpression;

        public Expression Expression { get; set; }

        public CheckedExpression(Expression checkedExpression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = checkedExpression;
            TextSpan = textSpan;
        }

        public CheckedExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { this };
        }
    }
}
