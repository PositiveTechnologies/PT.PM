using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternStatements : PatternUst<Ust>
    {
        public List<PatternUst> Statements { get; set; } = new List<PatternUst>();

        public PatternStatements()
        {
        }

        public PatternStatements(IEnumerable<PatternUst> statements, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Statements = statements?.ToList() ?? new List<PatternUst>();
        }

        public PatternStatements(params PatternUst[] statements)
        {
            Statements = statements?.ToList() ?? new List<PatternUst>();
        }

        public override string ToString() => string.Join(";\n", Statements);

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            var blockStatement = ust as BlockStatement;
            if (blockStatement == null ||
                (!(blockStatement.Parent is MethodDeclaration) &&
                 !(blockStatement.Parent is ConstructorDeclaration) &&
                 !(blockStatement.Parent is NamespaceDeclaration) &&
                 !(blockStatement.Parent is RootUst)))
            {
                return context.Fail();
            }

            MatchingContext newContext = MatchingContext.CreateWithInputParamsAndVars(context);

            if (Statements == null || Statements.Count == 0)
            {
                if (blockStatement.Statements == null || blockStatement.Statements.Count == 0)
                {
                    newContext = newContext.AddMatch(blockStatement);
                }
                else
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
                    .WhereDescendants(descendant =>
                        descendant is Expression expressionDescendant &&
                        !(expressionDescendant is Token)))
                    .Cast<Expression>()
                    .ToArray();

                var matchedTextSpans = new List<TextSpan>();
                int patternStatementInd = 0;
                bool success = false;
                for (int i = 0; i < expressions.Length; i++)
                {
                    newContext = MatchingContext.CreateWithInputParamsAndVars(newContext);
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

                if (success)
                {
                    context = context.AddMatches(matchedTextSpans);
                }
                else
                {
                    context = context.Fail();
                }
            }

            return context;
        }
    }
}
