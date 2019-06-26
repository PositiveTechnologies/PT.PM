using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class WrapperExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.WrapperExpression;

        [Key(UstFieldOffset)]
        public Ust Node { get; set; }

        public WrapperExpression(Ust node, TextSpan textSpan)
            : base(textSpan)
        {
            Node = node;
        }

        public WrapperExpression(Ust node)
            : base(node.TextSpan)
        {
            Node = node;
        }

        public WrapperExpression()
        {
        }

        public override Ust[] GetChildren() => new [] { Node };

        public override Expression[] GetArgs()
        {
            return new Expression[0];
        }

        public override string ToString()
        {
            return $"WrapExpr({Node})";
        }
    }
}
