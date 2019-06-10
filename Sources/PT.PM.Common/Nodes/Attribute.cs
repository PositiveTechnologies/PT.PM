using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class Attribute : Ust
    {
        [Key(0)] public override UstType UstType => UstType.Attribute;

        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        public Attribute(Expression expression, TextSpan textSpan = default)
            : base(textSpan)
        {
            Expression = expression;
        }

        public Attribute()
        {
        }

        public override Ust[] GetChildren()
        {
            return new[] { Expression };
        }

        public override string ToString()
        {
            return $"[{Expression}]";
        }
    }
}
