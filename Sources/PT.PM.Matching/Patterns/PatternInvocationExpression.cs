using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternInvocationExpression : PatternBase
    {
        public PatternBase Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public override Ust[] GetChildren() => new Ust[] { Target, Arguments };

        public PatternInvocationExpression()
        {
        }

        public PatternInvocationExpression(PatternBase target, PatternArgs arguments,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public override string ToString() => $"{Target}({Arguments})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;
            if (ust is InvocationExpression invocation)
            {
                match = Target.Match(invocation.Target, context);
                if (!match.Success)
                {
                    return match;
                }
                match = Arguments.Match(invocation.Arguments, match);
            }
            else
            {
                match = context.Fail();
            }
            return match.AddUstIfSuccess(ust);
        }
    }
}
