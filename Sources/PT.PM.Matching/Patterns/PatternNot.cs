using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternNot : PatternBase
    {
        public PatternBase Expression { get; set; }

        public PatternNot()
        {
        }

        public PatternNot(PatternBase expression, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = expression;
        }

        public override Ust[] GetChildren() => new Ust[] { Expression };

        public override string ToString() => $"<~>{Expression}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            MatchingContext match = Expression.Match(ust, context);
            return match.Change(!match.Success);
        }
    }
}
