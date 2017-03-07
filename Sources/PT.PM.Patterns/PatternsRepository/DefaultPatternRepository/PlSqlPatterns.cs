using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<Pattern> CreatePlSqlPatterns()
        {
            var patterns = new List<Pattern>();

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Dangerous Function",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Node = new InvocationExpression()
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdToken("DBMS_UTILITY"),
                            Name = new IdToken("EXEC_DDL_STATEMENT")
                        },
                        Arguments = new PatternExpressions()
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Node = new InvocationExpression()
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdToken("DBMS_OBFUSCATION_TOOLKIT"),
                            Name = new PatternIdToken("^(md2|md4|md5)$")
                        },
                        Arguments = new PatternExpressions()
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Node = new MemberReferenceExpression
                    {
                        Target = new IdToken("dbms_crypto"),
                        Name = new IdToken("hash_sh1")
                    }
                }
            });
            
            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Insecure Randomness",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new IdToken("DBMS_RANDOM"),
                            Name = new PatternIdToken()
                        },
                        Arguments = new PatternExpressions()
                    }
                }
            });

            var cursorVar = new PatternVarDef
            {
                Id = "cursor",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: Cursor Snarfing",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Vars = new List<PatternVarDef> { cursorVar },
                    Node = new PatternStatements
                    {
                        Statements = new Statement[]
                        {
                            new PatternExpressionInsideStatement
                            {
                                Statement = new ExpressionStatement(new AssignmentExpression
                                {
                                    Left = new PatternVarRef(cursorVar),
                                    Right = new MemberReferenceExpression
                                    {
                                        Target = new IdToken("DBMS_SQL"),
                                        Name = new IdToken("OPEN_CURSOR")
                                    }
                                })
                            },
                            new PatternMultipleStatements(),
                            new PatternExpressionInsideStatement
                            {
                                Statement = new ExpressionStatement(new InvocationExpression
                                {
                                    Target = new MemberReferenceExpression
                                    {
                                        Target = new IdToken("DBMS_SQL"),
                                        Name = new IdToken("CLOSE_CURSOR")
                                    },
                                    Arguments = new ArgsNode(new PatternVarRef(cursorVar))
                                }),
                                Not = true
                            }
                        }
                    }
                }
            });

            var fileVar = new PatternVarDef
            {
                Id = "file",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: File Snarfing",
                Languages = LanguageFlags.PlSql,
                Data = new PatternNode
                {
                    Vars = new List<PatternVarDef> { fileVar },
                    Node = new PatternStatements
                    {
                        Statements = new Statement[]
                        {
                            new PatternExpressionInsideStatement
                            {
                                Statement = new ExpressionStatement(new AssignmentExpression
                                {
                                    Left = new PatternVarRef(fileVar),
                                    Right = new InvocationExpression
                                    {
                                        Target = new MemberReferenceExpression
                                        {
                                            Target = new PatternIdToken("(?i)UTL_FILE"),
                                            Name = new PatternIdToken("(?i)FOPEN")
                                        },
                                        Arguments = new PatternExpressions()
                                    }
                                })
                            },
                            new PatternMultipleStatements(),
                            new PatternExpressionInsideStatement
                            {
                                Statement = new ExpressionStatement(new InvocationExpression
                                {
                                    Target = new MemberReferenceExpression
                                    {
                                        Target = new PatternIdToken("(?i)UTL_FILE"),
                                        Name = new PatternIdToken("(?i)FCLOSE")
                                    },
                                    Arguments = new ArgsNode(new PatternVarRef(fileVar))
                                }),
                                Not = true
                            }
                        }
                    }
                }
            });

            return patterns;
        }
    }
}
