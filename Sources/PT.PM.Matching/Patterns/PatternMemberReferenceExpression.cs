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
            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
