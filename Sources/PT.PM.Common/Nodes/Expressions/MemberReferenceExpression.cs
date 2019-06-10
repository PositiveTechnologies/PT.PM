using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class MemberReferenceExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.MemberReferenceExpression;

        [Key(UstFieldOffset)]
        public Expression Target { get; set; }

        [Key(UstFieldOffset + 1)]
        public Expression Name { get; set; }

        public MemberReferenceExpression()
        {
        }

        public MemberReferenceExpression(Expression target, Expression name, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public override Ust[] GetChildren() => new Ust[] { Target, Name };

        public override Expression[] GetArgs() => new Expression[] { Target, Name };

        public override string ToString()
        {
            return $"{Target}.{Name}";
        }
    }
}
