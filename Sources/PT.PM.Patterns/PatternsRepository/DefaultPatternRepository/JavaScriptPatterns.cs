using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<Pattern> CreateJavaScriptPatterns()
        {
            var patterns = new List<Pattern>();

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AttributesCodeInsideElementEvent",
                Languages = LanguageFlags.JavaScript,
                Data = new PatternNode
                {
                    Node = new AssignmentExpression
                    {
                        Left = new MemberReferenceExpression
                        {
                            Target = new PatternExpression(),
                            Name = new PatternIdToken("^on")
                        },
                        Right = new AnonymousMethodExpression
                        {
                            Body = new PatternStatements
                            {
                                Statements = new List<Statement>
                                {
                                    new ExpressionStatement
                                    {
                                        Expression = new PatternExpressionInsideExpression
                                        {
                                            Expression = new PatternVarDef
                                            {
                                                Id = "pt.pm_var_2",
                                                Values = new List<Expression>()
                                                {
                                                    new MemberReferenceExpression
                                                    {
                                                        Target = new IdToken("document"),
                                                        Name = new PatternIdToken("^(URL|referrer|cookie)$")
                                                    },
                                                    new PatternVarDef
                                                    {
                                                        Id = "pt.pm_var_1",
                                                        Values = new List<Expression>()
                                                        {
                                                            new MemberReferenceExpression
                                                            {
                                                                Target = new MemberReferenceExpression
                                                                {
                                                                    Target = new IdToken("document"),
                                                                    Name = new IdToken("location")
                                                                },
                                                                Name = new PatternIdToken("^(pathname|href|search|hash)$")
                                                            },
                                                            new PatternVarDef
                                                            {
                                                                Id = "pt.pm_var_0",
                                                                Values = new List<Expression>()
                                                                {
                                                                    new MemberReferenceExpression
                                                                    {
                                                                        Target = new IdToken("window"),
                                                                        Name = new IdToken("name")
                                                                    },
                                                                    new MemberReferenceExpression
                                                                    {
                                                                        Target = new MemberReferenceExpression
                                                                        {
                                                                            Target = new MemberReferenceExpression
                                                                            {
                                                                                Target = new MemberReferenceExpression
                                                                                {
                                                                                    Target = new IdToken("window"),
                                                                                    Name = new PatternIdToken("^(top|frames)$")
                                                                                },
                                                                                Name = new IdToken("document")
                                                                            },
                                                                            Name = new PatternIdToken()
                                                                        },
                                                                        Name = new PatternIdToken()
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return patterns;
        }
    }
}
