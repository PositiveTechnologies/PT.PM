using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class CheckedExpression : SpecificExpression
    {
        [Key(0)] public override UstType UstType => UstType.CheckedExpression;

        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        public CheckedExpression(Expression checkedExpression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = checkedExpression;
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

        public override string ToString()
        {
            return $"checked ({Expression})";
        }
    }
}
