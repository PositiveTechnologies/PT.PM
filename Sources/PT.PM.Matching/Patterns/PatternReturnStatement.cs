using PT.PM.Common;
using System;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Matching.Patterns
{
    public class PatternReturnStatement : PatternUst
    {
        public PatternUst Expression { get; set; } = new PatternAny();

        public PatternReturnStatement()
        {
        }

        public PatternReturnStatement(PatternUst pattern, TextSpan textSpan = default)
            : base(textSpan)
        {
            Expression = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        public override string ToString() => $"return {Expression}";

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var returnStatement = ust as ReturnStatement;
            if (returnStatement == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Expression.Match(returnStatement.Return, context);

            return newContext.Success
                ? newContext.AddMatch(returnStatement)
                : newContext.Fail();
        }
    }
}
