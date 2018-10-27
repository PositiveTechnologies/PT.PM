using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternNot : PatternUst
    {
        public PatternUst Pattern { get; set; } = new PatternAny();

        public PatternNot()
        {
        }

        public PatternNot(PatternUst pattern, TextSpan textSpan = default)
            : base(textSpan)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"<~>{Pattern}";

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            MatchContext newContext = Pattern.Match(ust, context);

            return newContext.Success
                ? newContext.Fail()
                : newContext.AddMatch(ust);
        }
    }
}
