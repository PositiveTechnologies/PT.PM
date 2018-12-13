using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.Matching.PatternsRepository;
using PT.PM.PhpParseTreeUst;
using PT.PM.SqlParseTreeUst;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository : MemoryPatternsRepository
    {
        private PatternIdGenerator patternIdGenerator = new PatternIdGenerator();

        public DefaultPatternRepository()
        {
        }

        protected override List<PatternDto> InitPatterns()
        {
            var patterns = new List<PatternRoot>();

            var commonPatterns = CreateCommonPatterns();
            patterns.AddRange(commonPatterns);
            var cSharpPatterns = CreateCSharpPatterns();
            patterns.AddRange(cSharpPatterns);
            var javaPatterns = CreateJavaPatterns();
            patterns.AddRange(javaPatterns);
            var phpPatterns = CreatePhpPatterns();
            patterns.AddRange(phpPatterns);
            var plSqlPatterns = CreatePlSqlPatterns();
            patterns.AddRange(plSqlPatterns);
            var tSqlPatterns = CreateTSqlPatterns();
            patterns.AddRange(tSqlPatterns);
            var javaScriptPatterns = CreateJavaScriptPatterns();
            patterns.AddRange(javaScriptPatterns);
            var htmpPatterns = CreateHtmlPatterns();
            patterns.AddRange(htmpPatterns);
            var clangsPatterns = CreateCLangsPatterns();
            patterns.AddRange(clangsPatterns);

            var patternsConverter = new PatternConverter();

            List<PatternDto> result = patternsConverter.ConvertBack(patterns.ToArray()).ToList();
            return result;
        }

        protected IEnumerable<PatternRoot> CreateCommonPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedPassword",
                Languages = new HashSet<Language>(LanguageUtils.PatternLanguages.Values),
                Node = new PatternOr
                (
                    new PatternBinaryOperatorExpression
                    {
                        Left = new PatternOr
                        (
                            new PatternIdRegexToken(@"(?i)(password|pwd)"),
                            new PatternMemberReferenceExpression
                            {
                                Target = new PatternAny(),
                                Name = new PatternIdRegexToken(@"(?i)(password|pwd)")
                            }
                        ),
                        Operator = new PatternBinaryOperatorLiteral(BinaryOperator.Equal),
                        Right = new PatternOr
                        (
                            new PatternStringRegexLiteral(),
                            new PatternNullLiteral()
                        )
                    },
                    new PatternAssignmentExpression
                    {
                        Left = new PatternOr
                        (
                            new PatternIdRegexToken(@"(?i)(password|pwd)"),
                            new PatternMemberReferenceExpression
                            {
                                Target = new PatternAny(),
                                Name = new PatternIdRegexToken(@"(?i)(password|pwd)")
                            }
                        ),
                        Right = new PatternOr
                        (
                            new PatternStringRegexLiteral(),
                            new PatternNullLiteral()
                        )
                    }
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureTransport",
                Languages = new HashSet<Language>
                {
                    CSharp.Language,
                    Java.Language,
                    Php.Language,
                    JavaScript.Language
                },
                Node = new PatternStringRegexLiteral(@"^http://[\w@][\w.:@]+/?[\w\.?=%&=\-@/$,]*$")
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureRandomness",
                Languages = new HashSet<Language>() { CSharp.Language, Java.Language },
                Node = new PatternObjectCreateExpression
                {
                    Type = new PatternIdToken("Random"),
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PasswordInComment. Storing passwords or password details in plaintext anywhere in the system or system code may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>(LanguageUtils.PatternLanguages.Values),
                Node = new PatternOr
                (
                    new PatternCommentRegex("(?i)(password|pwd)\\s*=\\s*[\"\\w]+"),
                    new PatternCommentRegex("(?i)default password")
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Poor Error Handling: Empty Default Exception Handler",
                Languages = new HashSet<Language>(LanguageUtils.PatternLanguages.Values.Where(x => x != MySql.Language)),
                Node = new PatternTryCatchStatement { IsCatchBodyEmpty = true }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Erroneous Null Comparison",
                Languages = new HashSet<Language>(LanguageUtils.SqlLanguages.Values),
                Node = new PatternOr
                (
                    new PatternBinaryOperatorExpression
                    {
                        Left = new PatternAny(),
                        Operator = new PatternBinaryOperatorLiteral(BinaryOperator.Equal),
                        Right = new PatternNullLiteral()
                    },
                    new PatternBinaryOperatorExpression
                    {
                        Left = new PatternAny(),
                        Operator = new PatternBinaryOperatorLiteral(BinaryOperator.NotEqual),
                        Right = new PatternNullLiteral()
                    }
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Privilege Management: Overly Broad Grant",
                Languages = new HashSet<Language>(LanguageUtils.SqlLanguages.Values),
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdToken("grant_all"),
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            return patterns;
        }
    }
}