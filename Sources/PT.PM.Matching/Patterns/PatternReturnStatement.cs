using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternReturnStatement : PatternUst
    {
        public PatternUst Expression { get; set; } = PatternAny.Instance;

        public PatternReturnStatement()
        {
        }

        public PatternReturnStatement(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"return {Expression}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ReturnStatement returnStatement)
            {
                newContext = Expression.Match(returnStatement.Return, context);
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
