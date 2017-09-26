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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return false;
            }

            bool match = !Expression.Match(ust, context);
            return match;
        }
    }
}
