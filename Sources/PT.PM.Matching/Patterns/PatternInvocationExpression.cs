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
            if (ust?.Kind != UstKind.InvocationExpression)
            {
                return context.Fail();
            }

            var invocationExpression = (InvocationExpression)ust;
            MatchingContext match = context;
            match = Target.Match(invocationExpression.Target, match);
            if (!match.Success)
            {
                return match;
            }

            match = Arguments.Match(invocationExpression.Arguments, match);
            return match;
        }
    }
}
