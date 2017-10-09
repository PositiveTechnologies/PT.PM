using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternThrow : PatternUst
    {
        public PatternUst Pattern { get; set; } = PatternAny.Instance;

        public PatternThrow()
        {
        }

        public PatternThrow(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"throw {Pattern}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            if (ust is ThrowStatement throwStatement)
            {
                MatchingContext newContext = Pattern.Match(throwStatement.ThrowExpression, context);
                if (newContext.Success)
                {
                    return newContext.AddMatch(ust);
                }
                else
                {
                    return newContext.Fail();
                }
            }

            return context.Fail();
        }
    }
}
