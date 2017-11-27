using PT.PM.Common;
using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Matching.Patterns
{
    public class PatternReturnStatement : PatternUst<ReturnStatement>
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

        public override MatchingContext Match(ReturnStatement returnStatement, MatchingContext context)
        {
            MatchingContext newContext = Expression.MatchUst(returnStatement.Return, context);
            return newContext.Success
                ? newContext.AddMatch(returnStatement)
                : newContext.Fail();
        }
    }
}
