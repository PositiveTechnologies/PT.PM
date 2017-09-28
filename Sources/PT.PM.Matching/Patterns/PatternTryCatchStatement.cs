using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternTryCatchStatement : PatternBase
    {
        public List<PatternBase> ExceptionTypes { get; set; }

        public bool IsCatchBodyEmpty { get; set; }

        public PatternTryCatchStatement()
        {
            ExceptionTypes = new List<PatternBase>();
            IsCatchBodyEmpty = true;
        }

        public PatternTryCatchStatement(IEnumerable<PatternBase> exceptionTypes, bool isCatchBodyEmpty,
            TextSpan textSpan)
            : base(textSpan)
        {
            ExceptionTypes = exceptionTypes?.ToList()
                ?? throw new ArgumentNullException("exceptionTypes");
            IsCatchBodyEmpty = isCatchBodyEmpty;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => $"try catch {{ }}";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is TryCatchStatement tryCatchStatement)
            {
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
                            ExceptionTypes.Any(type => type.Match(catchClause.Type, context).Success);
                    });

                    newContext = context.Change(result);
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddMatchIfSuccess(ust);
        }
    }
}
