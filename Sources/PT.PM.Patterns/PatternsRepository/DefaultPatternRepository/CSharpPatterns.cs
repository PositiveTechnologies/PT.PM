using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<Pattern> CreateCSharpPatterns()
        {
            var patterns = new List<Pattern>();

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash",
                Languages = LanguageFlags.CSharp,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("Create"),
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken("MD5"),
                                Target = new MemberReferenceExpression
                                {
                                    Name = new IdToken("Cryptography"),
                                    Target = new MemberReferenceExpression
                                    {
                                        Name = new IdToken("Security"),
                                        Target = new IdToken("System")
                                    }
                                }
                            }
                        },
                        Arguments = new PatternExpressions(new PatternMultipleExpressions())
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Use of NullReferenceException Catch to Detect NULL Pointer Dereference",
                Languages = LanguageFlags.CSharp,
                Data = new PatternNode
                {
                    Node = new PatternTryCatchStatement
                    {
                        ExceptionTypes = new List<TypeToken> {
                            new TypeToken("NullReferenceException"),
                            new TypeToken("System.NullReferenceException")
                    },
                        IsCatchBodyEmpty = false
                    }
                }
            });

            return patterns;
        }
    }
}
