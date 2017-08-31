using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Expressions
{
    public class MemberReferenceExpression : Expression
    {
        public override NodeType NodeType => NodeType.MemberReferenceExpression;

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

        public override UstNode[] GetChildren()
        {
            var result = new UstNode[] {Target, Name};
            return result;
        }

        public override string ToString()
        {
            return $"{Target}.{Name}";
        }
    }
}
