using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.TypeMembers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternStatements : PatternBase
    {
        public List<PatternBase> Statements { get; set; } = new List<PatternBase>();

        public PatternStatements()
        {
        }

        public PatternStatements(IEnumerable<PatternBase> statements, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Statements = statements?.ToList() ?? new List<PatternBase>();
        }

        public PatternStatements(params PatternBase[] statements)
        {
            Statements = statements?.ToList() ?? new List<PatternBase>();
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

            if (Statements == null || Statements.Count == 0)
            {
                return context;
            }

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
            MatchingContext newContext = MatchingContext.CreateWithInputParamsAndVars(context);
            for (int i = 0; i < expressions.Length; i++)
            {
                newContext = MatchingContext.CreateWithInputParamsAndVars(newContext);
                newContext = Statements[patternStatementInd].Match(expressions[i], newContext);
                if (newContext.Success)
                {
                    matchedTextSpans.AddRange(newContext.Locations);
                    patternStatementInd += 1;
                    if (patternStatementInd == Statements.Count)
                    {
                        success = true;
                        if (!context.FindAllAlternatives)
                        {
                            break;
                        }
                        else
                        {
                            patternStatementInd -= 1;
                        }
                    }
                }
            }

            return success
                ? context.AddMatches(matchedTextSpans)
                : context.Fail();
        }
    }
}
