using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateJavaScriptPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AttributesCodeInsideElementEvent",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternAssignmentExpression
                {
                    Left = new PatternMemberReferenceExpression
                    {
                        Target = new PatternAny(),
                        Name = new PatternIdRegexToken("^on")
                    },
                    Right = new PatternArbitraryDepth
                    (
                        new PatternOr
                        (
                            new PatternOr
                            (
                                new PatternOr
                                (
                                    new PatternMemberReferenceExpression
                                    {
                                        Target = new PatternIdToken("window"),
                                        Name = new PatternIdToken("name")
                                    },
                                    new PatternMemberReferenceExpression
                                    {
                                        Target = new PatternMemberReferenceExpression
                                        {
                                            Target = new PatternMemberReferenceExpression
                                            {
                                                Target = new PatternMemberReferenceExpression
                                                {
                                                    Target = new PatternIdToken("window"),
                                                    Name = new PatternIdRegexToken("^(top|frames)$")
                                                },
                                                Name = new PatternIdToken("document")
                                            },
                                            Name = new PatternIdRegexToken()
                                        },
                                        Name = new PatternIdRegexToken()
                                    }
                                ),

                                new PatternMemberReferenceExpression
                                {
                                    Target = new PatternMemberReferenceExpression
                                    {
                                        Target = new PatternIdToken("document"),
                                        Name = new PatternIdToken("location")
                                    },
                                    Name = new PatternIdRegexToken("^(pathname|href|search|hash)$")
                                }
                            ),

                            new PatternMemberReferenceExpression
                            {
                                Target = new PatternIdToken("document"),
                                Name = new PatternIdRegexToken("^(URL|referrer|cookie)$")
                            }
                        )
                    )
                }
            });

            // Node.js Patterns
            var nodeEP = new PatternOr
            (
                new PatternIdToken("req"),
                new PatternMemberReferenceExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("req"),
                        Name = new PatternIdToken("query")
                    },
                    Name = new PatternAny()
                },
                new PatternMemberReferenceExpression
                {
                    Target = new PatternIdToken("req"),
                    Name = new PatternIdToken("query")
                },
                new PatternMemberReferenceExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("req"),
                        Name = new PatternIdToken("body")
                    },
                    Name = new PatternAny()
                },
                new PatternMemberReferenceExpression
                {
                    Target = new PatternIdToken("req"),
                    Name = new PatternIdToken("body")
                },
                new PatternMemberReferenceExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("req"),
                        Name = new PatternIdToken("param")
                    },
                    Name = new PatternAny()
                },
                new PatternMemberReferenceExpression
                {
                    Target = new PatternIdToken("req"),
                    Name = new PatternIdToken("param")
                }
            );

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ServerSideInjectionEval",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdToken("eval"),
                    Arguments = new PatternArgs(nodeEP)
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ServerSideInjectionSetTimeout",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdToken("setTimeout"),
                    Arguments = new PatternArgs(nodeEP, new PatternMultipleExpressions()) // setTimeout(callback, delay[, ...args])
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ServerSideInjectionSetInterval",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdToken("setInterval"),
                    Arguments = new PatternArgs(nodeEP, new PatternMultipleExpressions()) // setInterval(callback, delay[, ...args])
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ServerSideInjectionFunction",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternObjectCreateExpression
                {
                    Type = new PatternIdToken("Function"),
                    Arguments = new PatternArgs(new PatternMultipleExpressions(), nodeEP) // new Function([arg1, ... , argN], body)
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ServerSideInjectionYaml",
                Languages = new HashSet<Language> { Language.JavaScript },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternOr
                        (
                            new PatternInvocationExpression
                            {
                                Target = new PatternIdToken("require"),
                                Arguments = new PatternArgs(new PatternStringRegexLiteral("^js-yaml$"))
                            },
                            new PatternIdToken("yaml")
                        ),
                        Name = new PatternIdToken("load")
                    },
                    Arguments = new PatternArgs(nodeEP, new PatternMultipleExpressions()) // load(string[, options])
                }
            });

            return patterns;
        }
    }
}
