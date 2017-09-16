using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootNode> CreateCSharpPatterns()
        {
            var patterns = new List<PatternRootNode>();

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash",
                Languages = new HashSet<Language>() { CSharp },
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
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Use of NullReferenceException Catch to Detect NULL Pointer Dereference",
                Languages = new HashSet<Language>() { CSharp },
                Node = new PatternTryCatchStatement
                {
                    ExceptionTypes = new List<Token> {
                            new TypeToken("NullReferenceException"),
                            new TypeToken("System.NullReferenceException")
                        },
                    IsCatchBodyEmpty = false
                }
            });

            return patterns;
        }
    }
}