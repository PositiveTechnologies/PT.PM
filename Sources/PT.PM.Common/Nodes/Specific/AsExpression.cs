using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Specific
{
    public class AsExpression : SpecificExpression
    {
        public Expression Expression { get; set; }

        public TypeToken Type { get; set; }

        public AsExpression(Expression expression, TypeToken type, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
            Type = type;
        }

        public AsExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Expression, Type };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Expression, Type };
        }

        public override string ToString()
        {
            return $"{Expression} as {Type}";
        }
    }
}
