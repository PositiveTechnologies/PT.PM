using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMemberReferenceExpression : PatternUst<MemberReferenceExpression>
    {
        public PatternUst Target { get; set; }

        public PatternUst Name { get; set; }

        public PatternMemberReferenceExpression()
        {
        }

        public PatternMemberReferenceExpression(PatternUst target, PatternUst name,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public override string ToString() => $"{Target}.{Name}";

        public override MatchContext Match(MemberReferenceExpression memberRef, MatchContext context)
        {
            MatchContext newContext = Target.MatchUst(memberRef.Target, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name.MatchUst(memberRef.Name, newContext);

            return newContext.AddUstIfSuccess(memberRef);
        }
    }
}
