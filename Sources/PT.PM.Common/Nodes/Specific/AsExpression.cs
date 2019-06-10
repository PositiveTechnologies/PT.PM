using MessagePack;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class AsExpression : SpecificExpression
    {
        [Key(0)] public override UstType UstType => UstType.AsExpression;

        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        [Key(UstFieldOffset + 1)]
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
