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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.IndexerExpression)
            {
                return false;
            }

            var invocationExpression = (IndexerExpression)ust;
            return Target.Match(invocationExpression.Target, context) &&
                   Arguments.Match(invocationExpression.Arguments, context);
        }
    }
}
