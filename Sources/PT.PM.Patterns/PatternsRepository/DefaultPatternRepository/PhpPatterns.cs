using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootNode> CreatePhpPatterns()
        {
            var patterns = new List<PatternRootNode>();

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedPasswordIn_mysql_connect. Hardcoded passwords could compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language> { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^mysql_connect$"),
                    Arguments = new ArgsNode
                    {
                        Collection = new List<Expression>()
                        {
                            new PatternExpression(),
                            new PatternExpression(),
                            new PatternStringLiteral()
                        }
                    }
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureRandomness. Standard pseudorandom number generators cannot withstand cryptographic attacks.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^(mt_rand|rand|uniqid|shuffle|lcg_value)$"),
                    Arguments = new PatternExpressions()
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyPermissiveCORSPolicyg. The program defines an overly permissive Cross-Origin Resource Sharing (CORS) policy.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^header$"),
                    Arguments = new ArgsNode(new[] { new PatternStringLiteral(@"Access-Control-Allow-Origin:\s*\*") })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InadequateRSAPadding. Public key RSA encryption is performed without using OAEP padding, thereby making the encryption weak.",
                Languages = new HashSet<Language>() { Php },
                Node = new IdToken("OPENSSL_NO_PADDING")
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "BrokenRiskyCryptographicAlgorithm. Weak Encryption: Broken or Risky Cryptographic Algorithm.",
                Languages = new HashSet<Language>() { Php },
                Node = new IdToken("MCRYPT_DES")
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash. Weak cryptographic hashes cannot guarantee data integrity and should not be used in security-critical contexts.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^(md5|sha1)$"),
                    Arguments = new PatternExpressions(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ExcessiveSessionTimeout. An overly long session timeout gives attackers more time to potentially compromise user accounts.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdToken("Configure"),
                        Name = new PatternIdToken("write")
                    },
                    Arguments = new ArgsNode(new[]
                    {
                        new StringLiteral("Security.level"),
                        new StringLiteral("low")
                    })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "DebugInformation. A CakePHP debug level of 1 or greater can cause sensitive data to be logged.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdToken("Configure"),
                        Name = new IdToken("write")
                    },
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new StringLiteral("debug"),
                        new PatternIntLiteral(1, 9)
                    })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SystemInformationLeak. Revealing system data or debugging information helps an adversary learn about the system and form a plan of attack.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)^(debug_print_backtrace|var_dump|debug_zval_dump|print_r|var_export|phpinfo|mysql_error)$"),
                    Arguments = new PatternExpressions()
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHashHardcodedSalt. A hardcoded salt may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)crypt"),
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new PatternExpression(),
                        new PatternStringLiteral()
                    })
                }
            });

            var encryptKeyVarName = new PatternVarDef
            {
                Id = "encryption_key",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            var encryptKeyVarValue = new PatternVarDef
            {
                Id = "encryption_key_value",
                Values = new List<Expression>
                {
                    new NullLiteral(),
                    new PatternStringLiteral()
                }
            };
            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "KeyManagementNullEncryptionKey. Null encryption keys may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>() { Php },
                Vars = new List<PatternVarDef> { encryptKeyVarName, encryptKeyVarValue },
                Node = new PatternStatements
                {
                    Statements = new List<Statement>()
                    {
                        new ExpressionStatement
                        {
                            Expression = new AssignmentExpression
                            {
                                Left = new PatternVarRef(encryptKeyVarName),
                                Right = new PatternVarRef(encryptKeyVarValue)
                            }
                        },

                        new PatternMultipleStatements(),

                        new ExpressionStatement
                        {
                            Expression = new AssignmentExpression
                            {
                                Left = new PatternExpression(),
                                Right =  new ObjectCreateExpression
                                {
                                    Type = new TypeToken("Zend_Filter_Encrypt"),
                                    Arguments = new ArgsNode
                                    {
                                        Collection = new List<Expression>() { new PatternVarRef(encryptKeyVarName) }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "KeyManagementNullEncryptionKey",
                Languages = new HashSet<Language>() { Php },
                Vars = new List<PatternVarDef> { encryptKeyVarValue },
                Node = new ObjectCreateExpression
                {
                    Type = new TypeToken("Zend_Filter_Encrypt"),
                    Arguments = new ArgsNode
                    {
                        Collection = new List<Expression>() { new PatternVarRef(encryptKeyVarValue) }
                    }
                }
            });

            // TODO: Union this next pattern.
            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityOverlyBroadPath. A cookie with an overly broad path can be accessed through other applications on the same domain.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)setcookie"),
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new StringLiteral("/"),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                    })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityOverlyBroadDomain. A cookie with an overly broad domain opens an application to attacks through other applications.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)setcookie"),
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternStringLiteral(@"^\..*"),
                        new PatternExpression(),
                        new PatternExpression(),
                    })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityHTTPOnlyNotSet. The program creates a cookie, but fails to set the HttpOnly flag to true.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)setcookie"),
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                    })
                }
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityCookieNotSentOverSSL. The program creates a cookie without setting the secure flag to true.",
                Languages = new HashSet<Language>() { Php },
                Node = new InvocationExpression
                {
                    Target = new PatternIdToken("(?i)setcookie"),
                    Arguments = new ArgsNode(new List<Expression>()
                    {
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression(),
                        new PatternExpression()
                    })
                }
            });

            return patterns;
        }
    }
}