using PT.PM.Common;
using PT.PM.Common.Nodes;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternNot : PatternBase
    {
        public PatternBase Pattern { get; set; } = PatternAny.Instance;

        public PatternNot()
        {
        }

        public PatternNot(PatternBase pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override Ust[] GetChildren() => new Ust[] { Pattern };

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
