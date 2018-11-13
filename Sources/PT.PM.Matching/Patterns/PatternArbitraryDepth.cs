using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternArbitraryDepth : PatternUst
    {
        public PatternUst Pattern { get; set; }

        public PatternArbitraryDepth()
        {
        }

        public PatternArbitraryDepth(PatternUst pattern, TextSpan textSpan = default)
            : base(textSpan)
        {
            Pattern = pattern;
        }

        public override string ToString()
        {
            if (Pattern == null)
            {
                return "#*";
            }

            return "<{ " + Pattern + " }>";
        }

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var result = ust.AnyDescendantOrSelf(ustNode => MatchExpression(ustNode, context).Success);
            return context.Set(result).AddUstIfSuccess(ust);
        }

        private MatchContext MatchExpression(Ust other, MatchContext context)
        {
            if (Pattern == null)
            {
                return other == null ? context : context.Fail();
            }

            return Pattern.MatchUst(other, context);
        }
    }
}
