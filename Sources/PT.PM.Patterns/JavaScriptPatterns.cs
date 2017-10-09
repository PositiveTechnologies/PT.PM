using PT.PM.Common;
using PT.PM.JavaScriptParseTreeUst;
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
                Languages = new HashSet<Language>() { JavaScript.Language },
                Node = new PatternAssignmentExpression
                {
                    Left = new PatternMemberReferenceExpression
                    {
                        Target = new PatternAnyExpression(),
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

            return patterns;
        }
    }
}
