using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternNot : PatternUst
    {
        public PatternUst Pattern { get; set; } = PatternAny.Instance;

        public PatternNot()
        {
        }

        public PatternNot(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"<~>{Pattern}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            MatchingContext newContext = Pattern.Match(ust, context);
            if (newContext.Success)
            {
                return newContext.Fail();
            }
            else
            {
                return newContext.AddMatch(ust);
            }
        }
    }
}
