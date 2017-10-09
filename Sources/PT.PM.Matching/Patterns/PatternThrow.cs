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
            MatchingContext newContext;

            if (ust is ThrowStatement throwStatement)
            {
                newContext = Pattern.Match(throwStatement.ThrowExpression, context);
                if (newContext.Success)
                {
                    newContext = newContext.AddMatch(ust);
                }
                else
                {
                    newContext = newContext.Fail();
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
