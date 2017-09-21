namespace PT.PM.Common.Nodes.Expressions
{
    public class MemberReferenceExpression : Expression
    {
        public override UstKind Kind => UstKind.MemberReferenceExpression;

        public Expression Target { get; set; }

        public Expression Name { get; set; }

        public MemberReferenceExpression(Expression target, Expression name, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public MemberReferenceExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Target, Name };
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { Target, Name };
        }

        public override string ToString()
        {
            return $"{Target}.{Name}";
        }
    }
}
