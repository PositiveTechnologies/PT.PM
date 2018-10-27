using PT.PM.Common;
using PT.PM.Common.Nodes.Statements;
using System;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternThrowStatement : PatternUst
    {
        public PatternUst Expression { get; set; } = new PatternAny();

        public PatternThrowStatement()
        {
        }

        public PatternThrowStatement(PatternUst pattern, TextSpan textSpan = default)
            : base(textSpan)
        {
            Expression = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"throw {Expression}";

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var throwStatement = ust as ThrowStatement;
            if (throwStatement == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Expression.Match(throwStatement.ThrowExpression, context);

            return newContext.Success
                ? newContext.AddMatch(throwStatement)
                : newContext.Fail();
        }
    }
}
