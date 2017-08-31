using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;
using System.Linq;
using static PT.PM.Common.Language;

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
            var patterns = new List<PatternRootNode>();

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

            var patternsConverter = new PatternConverter(new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef)));

            List<PatternDto> result = patternsConverter.ConvertBack(patterns.ToArray()).ToList();
            return result;
        }

        protected IEnumerable<PatternRootNode> CreateCommonPatterns()
        {
            var patterns = new List<PatternRootNode>();

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedPassword",
                Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages),
                Node = new PatternVarDef
                {
                    Values = new List<Expression>()
                    {
                        new AssignmentExpression
                        {
                            Left = new PatternVarDef
                            {
                                Id = "MemberReferenceOrLiteral",
                                Values = new List<Expression>
                                {
                                    new MemberReferenceExpression
                                    {
                                        Target = new PatternExpression(),
                                        Name = new PatternIdToken(@"(?i)(password|pwd)")
                                    },
                                    new PatternIdToken(@"(?i)(password|pwd)")
                                }
                            },
                            Right = new PatternVarDef
                            {
                                Id = "PasswordValue",
                                Values = new List<Expression>
                                {
                                    new NullLiteral(),
                                    new PatternStringLiteral(),
                                }
                            }
                        },
                        new BinaryOperatorExpression
                        {
                            Left = new PatternVarDef
                            {
                                Id = "MemberReferenceOrLiteral",
                                Values = new List<Expression>
                                {
                                    new MemberReferenceExpression
                                    {
                                        Target = new PatternExpression(),
                                        Name = new PatternIdToken(@"(?i)(password|pwd)")
                                    },
                                    new PatternIdToken(@"(?i)(password|pwd)")
                                }
                            },
                            Operator = new BinaryOperatorLiteral { BinaryOperator = BinaryOperator.Equal },
                            Right = new PatternVarDef
                            {
                                Id = "PasswordValue",
                                Values = new List<Expression>
                                {
                                    new NullLiteral(),
                                    new PatternStringLiteral(),
                                }
                            }
                        }
                    }
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureTransport",
                Languages = new HashSet<Language>(LanguageExt.AllGplPatternLanguages),
                Node = new PatternStringLiteral(@"^http://[\w@][\w.:@]+/?[\w\.?=%&=\-@/$,]*$")
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureRandomness",
                Languages = new HashSet<Language>() { CSharp, Java },
                Node = new ObjectCreateExpression
                {
                    Type = new TypeToken { TypeText = "Random" },
                    Arguments = new PatternExpressions(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PasswordInComment. Storing passwords or password details in plaintext anywhere in the system or system code may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages),
                Node = new PatternVarDef
                {
                    Values = new List<Expression>
                        {
                            new PatternComment
                            {
                                Comment = "(?i)(password|pwd)\\s*=\\s*[\"\\w]+"
                            },
                            new PatternComment
                            {
                                Comment = "(?i)default password"
                            }
                        }
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Poor Error Handling: Empty Default Exception Handler",
                Languages = new HashSet<Language>(LanguageExt.AllPatternLanguages),
                Node = new PatternTryCatchStatement()
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Erroneous Null Comparison",
                Languages = new HashSet<Language>(LanguageExt.AllSqlPatternLanguages),
                Node = new PatternVarDef
                {
                    Id = "compare_with_null",
                    Values = new List<Expression>
                    {
                         new BinaryOperatorExpression
                         {
                             Left = new PatternExpression(),
                             Operator = new BinaryOperatorLiteral { BinaryOperator = BinaryOperator.Equal },
                             Right = new NullLiteral()
                         },
                         new BinaryOperatorExpression
                         {
                             Left = new PatternExpression(),
                             Operator = new BinaryOperatorLiteral { BinaryOperator = BinaryOperator.NotEqual },
                             Right = new NullLiteral()
                         }
                    },
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Privilege Management: Overly Broad Grant",
                Languages = new HashSet<Language>(LanguageExt.AllSqlPatternLanguages),
                Node = new InvocationExpression
                {
                    Target = new IdToken("grant_all"),
                    Arguments = new PatternExpressions(new PatternMultipleExpressions())
                }
            });

            return patterns;
        }
    }

}