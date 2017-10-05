using PT.PM.Common;
using PT.PM.CSharpParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateCSharpPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash",
                Languages = new HashSet<LanguageInfo>() { CSharp.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("Create"),
                        Target = new PatternMemberReferenceExpression
                        {
                            Name = new PatternIdToken("MD5"),
                            Target = new PatternMemberReferenceExpression
                            {
                                Name = new PatternIdToken("Cryptography"),
                                Target = new PatternMemberReferenceExpression
                                {
                                    Name = new PatternIdToken("Security"),
                                    Target = new PatternIdToken("System")
                                }
                            }
                        }
                    },
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Use of NullReferenceException Catch to Detect NULL Pointer Dereference",
                Languages = new HashSet<LanguageInfo>() { CSharp.Language },
                Node = new PatternTryCatchStatement
                {
                    ExceptionTypes = new List<PatternUst>
                    {
                        new PatternIdToken("NullReferenceException"),
                        new PatternIdToken("System.NullReferenceException")
                    },
                    IsCatchBodyEmpty = false
                }
            });

            return patterns;
        }
    }
}