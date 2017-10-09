using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMemberReferenceExpression : PatternUst
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is MemberReferenceExpression memberRef)
            {
                newContext = Target.Match(memberRef.Target, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Name.Match(memberRef.Name, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
