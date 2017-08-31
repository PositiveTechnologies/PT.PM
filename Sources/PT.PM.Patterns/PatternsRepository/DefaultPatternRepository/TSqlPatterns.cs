using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootNode> CreateTSqlPatterns()
        {
            var patterns = new List<PatternRootNode>();

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Dangerous Function",
                Languages = new HashSet<Language>() { TSql },
                Node = new InvocationExpression()
                {
                    Target = new PatternIdToken("xp_cmdshell"),
                    Arguments = new PatternExpressions(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Insecure Randomness",
                Languages = new HashSet<Language>() { TSql },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^rand$"),
                    Arguments = new PatternExpressions()
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = new HashSet<Language>() { TSql },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^HashBytes$"),
                    Arguments = new PatternExpressions(
                        new PatternStringLiteral("(?i)^(md2|md4|md5)$"),
                        new PatternMultipleExpressions())
                }
            });

            var cursorVar = new PatternVarDef
            {
                Id = "cursor",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: Cursor Snarfing",
                Languages = new HashSet<Language>() { TSql },
                Vars = new List<PatternVarDef> { cursorVar },
                Node = new PatternStatements
                {
                    Statements = new List<Statement>()
                    {
                        new PatternExpressionInsideStatement
                        {
                            Statement = new ExpressionStatement(new InvocationExpression
                            {
                                Target = new PatternIdToken("(?i)^declare_cursor$"),
                                Arguments = new ArgsNode(new PatternVarRef(cursorVar), new PatternMultipleExpressions())
                            })
                        },
                        new PatternExpressionInsideStatement
                        {
                            Statement = new ExpressionStatement(new InvocationExpression
                            {
                                Target = new PatternIdToken("(?i)^deallocate$"),
                                Arguments = new ArgsNode(new PatternVarRef(cursorVar))
                            }),
                            Not = true
                        }
                    }
                }
            });

            return patterns;
        }
    }
}
