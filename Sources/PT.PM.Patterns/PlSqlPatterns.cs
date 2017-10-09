using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.SqlParseTreeUst;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreatePlSqlPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Dangerous Function",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternInvocationExpression()
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("DBMS_UTILITY"),
                        Name = new PatternIdToken("EXEC_DDL_STATEMENT")
                    },
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternInvocationExpression()
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("DBMS_OBFUSCATION_TOOLKIT"),
                        Name = new PatternIdRegexToken("^(md2|md4|md5)$")
                    },
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternMemberReferenceExpression
                {
                    Target = new PatternIdToken("dbms_crypto"),
                    Name = new PatternIdToken("hash_sh1")
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Insecure Randomness",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("DBMS_RANDOM"),
                        Name = new PatternIdRegexToken()
                    },
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: Cursor Snarfing",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternStatements
                (
                    new PatternAssignmentExpression
                    {
                        Left = new PatternVar("cursor"),
                        Right = new PatternMemberReferenceExpression
                        {
                            Target = new PatternIdToken("DBMS_SQL"),
                            Name = new PatternIdToken("OPEN_CURSOR")
                        }
                    },

                    new PatternNot
                    (
                        new PatternInvocationExpression
                        {
                            Target = new PatternMemberReferenceExpression
                            {
                                Target = new PatternIdToken("DBMS_SQL"),
                                Name = new PatternIdToken("CLOSE_CURSOR")
                            },
                            Arguments = new PatternArgs(new PatternVar("cursor"))
                        }
                    )
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: File Snarfing",
                Languages = new HashSet<Language>() { PlSql.Language },
                Node = new PatternStatements
                (
                    new PatternArbitraryDepth
                    (
                        new PatternAssignmentExpression
                        {
                            Left = new PatternVar("file"),
                            Right = new PatternInvocationExpression
                            {
                                Target = new PatternMemberReferenceExpression
                                {
                                    Target = new PatternIdRegexToken("(?i)UTL_FILE"),
                                    Name = new PatternIdRegexToken("(?i)FOPEN")
                                },
                                Arguments = new PatternArgs(new PatternMultipleExpressions())
                            }
                        }
                    ),

                    new PatternArbitraryDepth
                    (
                        new PatternNot
                        (
                            new PatternInvocationExpression
                            {
                                Target = new PatternMemberReferenceExpression
                                {
                                    Target = new PatternIdRegexToken("(?i)UTL_FILE"),
                                    Name = new PatternIdRegexToken("(?i)FCLOSE")
                                },
                                Arguments = new PatternArgs(new PatternVar("file"))
                            }
                        )
                    )
                )
            });

            return patterns;
        }
    }
}