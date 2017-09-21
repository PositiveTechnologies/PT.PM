using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Expressions
{
    public class CastExpression : Expression
    {
        public override UstKind Kind => UstKind.CastExpression;

        public TypeToken Type { get; set; }

        public Expression Expression { get; set; }

        public CastExpression(TypeToken type, Expression expression, TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            Expression = expression;
        }

        public CastExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Type, Expression };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Type, Expression };
        }

        public override string ToString()
        {
            return $"({Type}){Expression}";
        }
    }
}
