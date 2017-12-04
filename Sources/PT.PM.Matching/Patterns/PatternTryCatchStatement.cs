using PT.PM.Common;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternTryCatchStatement : PatternUst<TryCatchStatement>
    {
        public List<PatternUst> ExceptionTypes { get; set; }

        public bool IsCatchBodyEmpty { get; set; }

        public PatternTryCatchStatement()
        {
            ExceptionTypes = new List<PatternUst>();
        }

        public PatternTryCatchStatement(IEnumerable<PatternUst> exceptionTypes, bool isCatchBodyEmpty,
            TextSpan textSpan)
            : base(textSpan)
        {
            ExceptionTypes = exceptionTypes?.ToList()
                ?? throw new ArgumentNullException("exceptionTypes");
            IsCatchBodyEmpty = isCatchBodyEmpty;
        }

        public override string ToString() => $"try catch {{ }}";

        public override MatchContext Match(TryCatchStatement tryCatchStatement, MatchContext context)
        {
            MatchContext newContext;

            if (tryCatchStatement.CatchClauses == null)
            {
                newContext = context.Fail();
            }
            else
            {
                bool result = tryCatchStatement.CatchClauses.Any(catchClause =>
                {
                    if (IsCatchBodyEmpty && catchClause.Body.Statements.Any())
                    {
                        return false;
                    }

                    return !ExceptionTypes.Any() ||
                        ExceptionTypes.Any(type => type.MatchUst(catchClause.Type, context).Success);
                });

                newContext = context.Set(result);
            }

            return newContext.AddUstIfSuccess(tryCatchStatement);
        }
    }
}
