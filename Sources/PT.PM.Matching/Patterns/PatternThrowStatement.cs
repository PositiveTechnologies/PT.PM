using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternThrowStatement : PatternUst
    {
        public PatternUst Expression { get; set; } = PatternAny.Instance;

        public PatternThrowStatement()
        {
        }

        public PatternThrowStatement(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"throw {Expression}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ThrowStatement throwStatement)
            {
                newContext = Expression.Match(throwStatement.ThrowExpression, context);
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
