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
            MatchingContext newContext;
            if (ust is InvocationExpression invocation)
            {
                newContext = Target.Match(invocation.Target, context);
                if (!newContext.Success)
                {
                    return newContext;
                }
                newContext = Arguments.Match(invocation.Arguments, newContext);
            }
            else
            {
                newContext = context.Fail();
            }
            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
