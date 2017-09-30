using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootUst> CreatePhpPatterns()
        {
            var patterns = new List<PatternRootUst>();

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedPasswordIn_mysql_connect. Hardcoded passwords could compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language> { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^mysql_connect$"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternStringRegexLiteral()
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureRandomness. Standard pseudorandom number generators cannot withstand cryptographic attacks.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^(mt_rand|rand|uniqid|shuffle|lcg_value)$"),
                    Arguments = new PatternArgs()
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyPermissiveCORSPolicyg. The program defines an overly permissive Cross-Origin Resource Sharing (CORS) policy.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^header$"),
                    Arguments = new PatternArgs(new PatternStringRegexLiteral(@"Access-Control-Allow-Origin:\s*\*"))
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InadequateRSAPadding. Public key RSA encryption is performed without using OAEP padding, thereby making the encryption weak.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternIdToken("OPENSSL_NO_PADDING")
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "BrokenRiskyCryptographicAlgorithm. Weak Encryption: Broken or Risky Cryptographic Algorithm.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternIdToken("MCRYPT_DES")
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash. Weak cryptographic hashes cannot guarantee data integrity and should not be used in security-critical contexts.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^(md5|sha1)$"),
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ExcessiveSessionTimeout. An overly long session timeout gives attackers more time to potentially compromise user accounts.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("Configure"),
                        Name = new PatternIdRegexToken("write")
                    },
                    Arguments = new PatternArgs
                    (
                        new PatternStringLiteral("Security.level"),
                        new PatternStringLiteral("low")
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "DebugInformation. A CakePHP debug level of 1 or greater can cause sensitive data to be logged.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Target = new PatternIdToken("Configure"),
                        Name = new PatternIdToken("write")
                    },
                    Arguments = new PatternArgs
                    (
                        new PatternStringLiteral("debug"),
                        new PatternIntRangeLiteral(1, 9)
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SystemInformationLeak. Revealing system data or debugging information helps an adversary learn about the system and form a plan of attack.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)^(debug_print_backtrace|var_dump|debug_zval_dump|print_r|var_export|phpinfo|mysql_error)$"),
                    Arguments = new PatternArgs()
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHashHardcodedSalt. A hardcoded salt may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)crypt"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternStringRegexLiteral()
                    )
                }
            });

            var patternNullOrString = new PatternOr
            (
                new PatternStringRegexLiteral(),
                new PatternNullLiteral()
            );

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "KeyManagementNullEncryptionKey. Null encryption keys may compromise system security in a way that cannot be easily remedied.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternStatements
                (
                    new PatternAssignmentExpression
                    {
                        Left = new PatternVar("encryption_key"),
                        Right = patternNullOrString
                    },
                    new PatternAssignmentExpression
                    {
                        Left = new PatternAnyExpression(),
                        Right =  new PatternObjectCreateExpression
                        {
                            Type = new PatternIdToken("Zend_Filter_Encrypt"),
                            Arguments = new PatternArgs(new PatternVar("encryption_key"))
                        }
                    }
                )
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "KeyManagementNullEncryptionKey",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternObjectCreateExpression
                {
                    Type = new PatternIdToken("Zend_Filter_Encrypt"),
                    Arguments = new PatternArgs(patternNullOrString)
                }
            });

            // TODO: Union this next pattern.
            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityOverlyBroadPath. A cookie with an overly broad path can be accessed through other applications on the same domain.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)setcookie"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternStringLiteral("/"),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression()
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityOverlyBroadDomain. A cookie with an overly broad domain opens an application to attacks through other applications.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)setcookie"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternStringRegexLiteral(@"^\..*"),
                        new PatternAnyExpression(),
                        new PatternAnyExpression()
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityHTTPOnlyNotSet. The program creates a cookie, but fails to set the HttpOnly flag to true.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)setcookie"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression()
                    )
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieSecurityCookieNotSentOverSSL. The program creates a cookie without setting the secure flag to true.",
                Languages = new HashSet<Language>() { Php },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("(?i)setcookie"),
                    Arguments = new PatternArgs
                    (
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression(),
                        new PatternAnyExpression()
                    )
                }
            });

            return patterns;
        }
    }
}