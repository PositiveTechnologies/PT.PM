using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Nodes.Collections;

namespace PT.PM.Matching.Patterns
{
    public class PatternStatements : PatternUst
    {
        public List<PatternUst> Statements { get; set; } = new List<PatternUst>();

        public PatternStatements()
        {
        }

        public PatternStatements(IEnumerable<PatternUst> statements, TextSpan textSpan = default)
            : base(textSpan)
        {
            Statements = statements?.ToList() ?? new List<PatternUst>();
        }

        public PatternStatements(params PatternUst[] statements)
        {
            Statements = statements?.ToList() ?? new List<PatternUst>();
        }

        public override string ToString() => string.Join(";\n", Statements);

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var blockStatement = ust as BlockStatement;
            Ust parent = ust?.Parent;

            if (blockStatement == null || parent == null)
            {
                return context.Fail();
            }

            if (!(parent is MethodDeclaration || parent is NamespaceDeclaration || parent is RootUst || parent is Collection))
            {
                return context.Fail();
            }

            MatchContext newContext = MatchContext.CreateWithInputParamsAndVars(context);

            if (Statements == null || Statements.Count == 0)
            {
                if (blockStatement.Statements?.Count > 0)
                {
                    return context.Fail();
                }
            }
            else
            {
                IEnumerable<Statement> statements = blockStatement.Statements
                    .Where(statement =>
                        !(statement is TypeDeclarationStatement) &&
                        !(statement is WrapperStatement));
                Expression[] expressions = statements.SelectMany(statement =>
                    statement
                    .WhereDescendantsOrSelf(descendant =>
                        descendant is Expression expressionDescendant &&
                        !(expressionDescendant is Token)))
                    .Cast<Expression>()
                    .ToArray();

                var matchedTextSpans = new List<TextSpan>();
                int patternStatementInd = 0;
                bool success = false;
                for (int i = 0; i < expressions.Length; i++)
                {
                    newContext = MatchContext.CreateWithInputParamsAndVars(newContext);
                    newContext = Statements[patternStatementInd].MatchUst(expressions[i], newContext);
                    if (newContext.Success)
                    {
                        matchedTextSpans.AddRange(newContext.Locations);
                        patternStatementInd += 1;
                        if (patternStatementInd == Statements.Count)
                        {
                            success = true;
                            patternStatementInd = 0;
                        }
                    }
                }

                context = success ? context.AddMatches(matchedTextSpans) : context.Fail();
            }

            return context;
        }
    }
}
