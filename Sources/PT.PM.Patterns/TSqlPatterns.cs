using PT.PM.Common;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootUst> CreateTSqlPatterns()
        {
            var patterns = new List<PatternRootUst>();

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Dangerous Function",
                Languages = new HashSet<Language>() { TSql },
                Node = new PatternInvocationExpression()
                {
                    Target = new PatternIdRegexToken("xp_cmdshell"),
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Insecure Randomness",
                Languages = new HashSet<Language>() { TSql },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^rand$"),
                    Arguments = new PatternArgs()
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Languages = new HashSet<Language>() { TSql },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^HashBytes$"),
                    Arguments = new PatternArgs
                    (
                        new PatternStringRegexLiteral("(?i)^(md2|md4|md5)$"),
                        new PatternMultipleExpressions()
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Unreleased Resource: Cursor Snarfing",
                Languages = new HashSet<Language>() { TSql },
                Node = new PatternStatements
                (
                    new PatternExpressionInside
                    {
                        Expression = new PatternInvocationExpression
                        {
                            Target = new PatternIdRegexToken("(?i)^declare_cursor$"),
                            Arguments = new PatternArgs(new PatternVar("cursor"), new PatternMultipleExpressions())
                        }
                    },
                    new PatternExpressionInside
                    {
                        Expression = new PatternNot(new PatternInvocationExpression
                        {
                            Target = new PatternIdRegexToken("(?i)^deallocate$"),
                            Arguments = new PatternArgs(new PatternVar("cursor"))
                        })
                    }
                )
            });

            return patterns;
        }
    }
}
