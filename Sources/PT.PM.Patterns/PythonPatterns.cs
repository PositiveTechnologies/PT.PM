using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreatePythonPatterns()
        {
            var pythonLang = new HashSet<Language> { Language.Python };
            var patterns = new List<PatternRoot>
            {
                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "HMAC weak hash",
                    Languages = pythonLang,
                    Node = new PatternAssignmentExpression
                    {
                        Left = new PatternAny(),
                        Right = new PatternInvocationExpression
                        {
                            Target = new PatternIdToken("pbkdf2_hmac"),
                            Arguments = new PatternArgs(new PatternStringLiteral("md5"),
                            new PatternAny(),
                            new PatternAny(),
                            new PatternAny())
                        }
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "Hardcoded comparison",
                    Node = new PatternBinaryOperatorExpression
                    {
                        Left = new PatternIdRegexToken(),
                        Operator = new PatternBinaryOperatorLiteral
                        {
                            BinaryOperator = BinaryOperator.Equal
                        },
                        Right = new PatternStringRegexLiteral(".+")
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "Hardcoded initialization vector",
                    Languages = pythonLang,
                    Node = new PatternInvocationExpression
                    {
                        Target = new PatternMemberReferenceExpression
                        {
                            Target = new PatternIdToken("AES"),
                            Name = new PatternIdToken("new")
                        },
                        Arguments = new PatternArgs(new PatternAny(),
                        new PatternMultipleExpressions(),
                        new PatternAssignmentExpression(
                            new PatternIdRegexToken("(?i)iv"),
                            new PatternStringRegexLiteral()))
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "Logging to STD output",
                    Languages = pythonLang,
                    Node = new PatternInvocationExpression
                    {
                        Target = new PatternMemberReferenceExpression
                        {
                            Target = new PatternMemberReferenceExpression{
                                Target = new PatternIdToken("sys"),
                                Name = new PatternIdToken("stderr")
                            },
                            Name = new PatternIdToken("write")
                        },
                        Arguments = new PatternArgs(new PatternMultipleExpressions())
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "XSS protection off",
                    Languages = pythonLang,
                    Node = new PatternAssignmentExpression
                    {
                        Left = new PatternIdToken("SECURE_BROWSER_XSS_FILTER"),
                        Right = new PatternBooleanLiteral(false)
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "Insecure connection protocol",
                    Languages = pythonLang,
                    Node = new PatternInvocationExpression
                    {
                        Target = new PatternMemberReferenceExpression
                        {
                            Target = new PatternIdToken("cfg"),
                            Name = new PatternIdToken("StrOpt")
                        },
                        Arguments = new PatternArgs(new PatternMultipleExpressions(),
                        new PatternAssignmentExpression(
                            new PatternIdToken("default"),
                            new PatternStringLiteral("http")),
                        new PatternMultipleExpressions()
                        )
                    }
                },

                new PatternRoot
                {
                    Key = patternIdGenerator.NextId(),
                    DebugInfo = "Insecure cipher key export",
                    Languages = pythonLang,
                    Node = new PatternInvocationExpression
                    {
                        Target = new PatternMemberReferenceExpression
                        {
                            Target = new PatternAny(),
                            Name =  new PatternIdToken("exportKey")
                        },
                        Arguments = new PatternArgs(new PatternMultipleExpressions(),
                                new PatternAssignmentExpression(
                                    new PatternIdToken("format"),
                                    new PatternStringLiteral("PEM")),
                                new PatternMultipleExpressions())
                    }
                }
            };

            return patterns;
        }
    }
}
