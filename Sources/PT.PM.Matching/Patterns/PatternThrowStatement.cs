using PT.PM.Common;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternThrowStatement : PatternUst<ThrowStatement>
    {
        public PatternUst Expression { get; set; } = new PatternAny();

        public PatternThrowStatement()
        {
        }

        public PatternThrowStatement(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"throw {Expression}";

        public override MatchContext Match(ThrowStatement throwStatement, MatchContext context)
        {
            MatchContext newContext = Expression.MatchUst(throwStatement.ThrowExpression, context);

            return newContext.Success
                ? newContext.AddMatch(throwStatement)
                : newContext.Fail();
        }
    }
}
