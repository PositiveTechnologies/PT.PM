using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMemberReferenceExpression : PatternBase
    {
        public PatternBase Target { get; set; }

        public PatternBase Name { get; set; }

        public PatternMemberReferenceExpression()
        {
        }

        public PatternMemberReferenceExpression(PatternBase target, PatternBase name,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public override Ust[] GetChildren() => new Ust[] { Target, Name };

        public override string ToString() => $"{Target}.{Name}";

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.MemberReferenceExpression)
            {
                return false;
            }

            var memberRef = (MemberReferenceExpression)ust;
            return Target.Match(memberRef.Target, context) &&
                     Name.Match(memberRef.Name, context);
        }
    }
}
