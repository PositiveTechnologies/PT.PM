using MessagePack;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class CastExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.CastExpression;

        [Key(UstFieldOffset)]
        public TypeToken Type { get; set; }

        [Key(UstFieldOffset + 1)]
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
