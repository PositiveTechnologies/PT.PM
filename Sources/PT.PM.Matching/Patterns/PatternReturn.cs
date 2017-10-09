using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternReturn : PatternUst
    {
        public PatternUst Pattern { get; set; } = PatternAny.Instance;

        public PatternReturn()
        {
        }

        public PatternReturn(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"return {Pattern}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            if (ust is ReturnStatement returnStatement)
            {
                MatchingContext newContext = Pattern.Match(returnStatement.Return, context);
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
