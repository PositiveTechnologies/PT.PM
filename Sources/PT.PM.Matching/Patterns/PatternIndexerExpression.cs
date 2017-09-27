using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIndexerExpression : PatternBase
    {
        public PatternBase Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public override Ust[] GetChildren() => new Ust[] { Target, Arguments };

        public PatternIndexerExpression()
        {
        }

        public PatternIndexerExpression(PatternBase target, PatternArgs arguments, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public override string ToString() => $"{Target}[{Arguments}]";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is IndexerExpression invocationExpression)
            {
                newContext = Target.Match(invocationExpression.Target, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Arguments.Match(invocationExpression.Arguments, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
