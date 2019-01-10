using PT.PM.Common;
using PT.PM.JavaParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateJavaPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InadequateRsaPadding. Weak Encryption: Inadequate RSA Padding. ",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("getInstance"),
                        Target = new PatternMemberReferenceExpression
                        {
                            Name = new PatternIdToken("Cipher"),
                            Target = new PatternMemberReferenceExpression
                            {
                                Name = new PatternIdToken("crypto"),
                                Target = new PatternIdToken("javax")
                            }
                        }
                    },
                    Arguments = new PatternArgs(new PatternStringRegexLiteral("^RSA/NONE/NoPadding$"))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicAlgorithm. Weak Encryption: Broken or Risky Cryptographic Algorithm" +
                    "https://cwe.mitre.org/data/definitions/327.html",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("getInstance"),
                        Target = new PatternMemberReferenceExpression
                        {
                            Name = new PatternIdToken("Cipher"),
                            Target = new PatternMemberReferenceExpression
                            {
                                Name = new PatternIdToken("crypto"),
                                Target = new PatternIdToken("javax")
                            }
                        }
                    },
                    Arguments = new PatternArgs(new PatternStringRegexLiteral(@"DES"))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadPath. Cookie Security: Overly Broad Path.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("setPath"),
                        Target = new PatternIdRegexToken(@"[cC]ookie")
                    },
                    Arguments = new PatternArgs(new PatternStringRegexLiteral("^/?$"))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadDomain Cookie Security: Overly Broad Domain.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("setDomain"),
                        Target = new PatternIdRegexToken("[cC]ookie")
                    },
                    Arguments = new PatternArgs(new PatternStringRegexLiteral(@"^.?[a-zA-Z0-9\-]+\.[a-zA-Z0-9\-]+$"))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PoorSeeding.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("setSeed"),
                        Target = new PatternAny()
                    },
                    Arguments = new PatternArgs(new PatternIntRangeLiteral())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("getInstance"),
                        Target = new PatternIdToken("MessageDigest")
                    },
                    Arguments = new PatternArgs(new PatternStringRegexLiteral("MD5|SHA-1"))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidPermissionCheck. Often Misused: Android Permission Check.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("^(checkCallingOrSelfPermission|checkCallingOrSelfUriPermission)$"),
                        Target = new PatternAny()
                    },
                    Arguments = new PatternArgs(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidHostnameVerificationDisabled. Insecure SSL: Android Hostname Verification Disabled.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternOr
                (
                    new PatternObjectCreateExpression
                    {
                        Type = new PatternIdToken("AllowAllHostnameVerifier"),
                        Arguments = new PatternArgs(new PatternMultipleExpressions())
                    },
                    new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("ALLOW_ALL_HOSTNAME_VERIFIER"),
                        Target = new PatternIdToken("SSLSocketFactory")
                    }
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SAXReaderExternalEntity",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs(new PatternNot(new PatternStringRegexLiteral())),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("read"),
                        Target = new PatternObjectCreateExpression
                        {
                            Type = new PatternIdToken("SAXReader"),
                            Arguments = new PatternArgs()
                        }
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "XmlExternalEntity",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs(new PatternNot(new PatternStringRegexLiteral())),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("parse"),
                        Target = new PatternObjectCreateExpression
                        {
                            Type = new PatternIdToken("XMLUtil"),
                            Arguments = new PatternArgs()
                        }
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "StickyBroadcast. Android Bad Practices: Sticky Broadcast.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs(new PatternAny()),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("sendStickyBroadcast"),
                        Target = new PatternAny()
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SendStickyBroadcastAsUser. Android Bad Practices: Sticky Broadcast.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs
                    (
                        new PatternAny(),
                        new PatternAny()
                    ),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("sendStickyBroadcastAsUser"),
                        Target = new PatternAny()
                    }
                }
            });

            // TODO: implement "createSocket"
            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureSSL. Insecure SSL: Android Socket.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs
                    (
                        new PatternAny(),
                        new PatternAny()
                    ),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("getInsecure"),
                        Target = new PatternAny()
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedSalt. Weak Cryptographic Hash: Hardcoded Salt.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs
                    (
                        new PatternAny(),
                        new PatternStringRegexLiteral()
                    ),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("hash"),
                        Target = new PatternAny()
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingReceiverPermission. The program sends a broadcast without specifying the receiver permission. " +
                            "Broadcasts sent without the receiver permission are accessible to any receiver. If these broadcasts contain sensitive data or reach a malicious receiver, the application may be compromised.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs(new PatternAny()),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("sendBroadcast"),
                        Target = new PatternAny()
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingBroadcasterPermission. The program registers a receiver without specifying the broadcaster permission. " +
                    "Receiver registered without the broadcaster permission will receive messages from any broadcaster. " +
                    "If these messages contain malicious data or come from a malicious broadcaster, the application may be compromised. " +
                    "Use this form: public abstract Intent registerReceiver (BroadcastReceiver receiver, IntentFilter filter, String broadcastPermission, Handler scheduler)",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternInvocationExpression
                {
                    Arguments = new PatternArgs
                    (
                        new PatternAny(),
                        new PatternAny()
                    ),
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdToken("registerReceiver"),
                        Target = new PatternAny()
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieNotSentOverSSL. Cookie Security: Cookie not Sent Over SSL.",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternStatements
                (
                    new PatternVarOrFieldDeclaration
                    {
                        LocalVariable = true,
                        Type = new PatternIdToken("Cookie"),
                        Assignment = new PatternAssignmentExpression
                        {
                            Left = new PatternVar("cookie"),
                            Right = new PatternObjectCreateExpression
                            {
                                Type = new PatternIdToken("Cookie"),
                                Arguments = new PatternArgs(new PatternMultipleExpressions())
                            },
                        }
                    },

                    /*new PatternNot
                    (
                        new PatternInvocationExpression
                        {
                            Arguments = new PatternArgs(new PatternBooleanLiteral(true)),
                            Target = new PatternMemberReferenceExpression
                            {
                                Name = new PatternIdToken("setSecure"),
                                Target = new PatternVar("cookie")
                            }
                        }
                    ),*/

                    new PatternInvocationExpression
                    {
                        Arguments = new PatternArgs(new PatternVar("cookie")),
                        Target = new PatternMemberReferenceExpression
                        {
                            Name = new PatternIdToken("addCookie"),
                            Target = new PatternAny()
                        }
                    }
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Use of NullPointerException Catch to Detect NULL Pointer Dereference",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternTryCatchStatement
                {
                    ExceptionTypes = new List<PatternUst> { new PatternIdToken("NullPointerException") },
                    IsCatchBodyEmpty = false
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "UsingCloneWithoutCloneable. Using clone method without implementing Clonable",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternAnd
                (
                    new PatternClassDeclaration
                    {
                        Body = new PatternArbitraryDepth
                        {
                            Pattern = new PatternMethodDeclaration
                            {
                                Name = new PatternIdToken("clone"),
                                AnyBody = true
                            }
                        }
                    },

                    new PatternNot
                    (
                        new PatternClassDeclaration
                        {
                            BaseTypes = new List<PatternUst> { new PatternIdToken("Cloneable") }
                        }
                    )
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ExtendingSecurityManagerWithoutFinal. Class extending SecurityManager is not final",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternAnd
                (
                    new PatternClassDeclaration
                    {
                        BaseTypes = new List<PatternUst>
                        {
                            new PatternIdToken("SecurityManager")
                        }
                    },

                    new PatternNot
                    (
                        new PatternClassDeclaration
                        {
                            Modifiers = new List<PatternUst>
                            {
                                new PatternIdToken("final")
                            }
                        }
                    )
                )
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ImproperValidationEmptyMethodFull. Improper Certificate Validation (Empty method)",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternClassDeclaration
                {
                    BaseTypes = new List<PatternUst>
                    {
                        new PatternIdRegexToken("X509TrustManager|SSLSocketFactory")
                    },
                    Body = new PatternArbitraryDepth
                    {
                        Pattern = new PatternOr
                        {
                            Patterns = new List<PatternUst>
                            {
                                new PatternMethodDeclaration(new List<PatternUst>(), new PatternIdRegexToken(@"\w+"), false),
                                new PatternOr
                                {
                                    Patterns = new List<PatternUst>
                                    {
                                        new PatternMethodDeclaration
                                        {
                                            Modifiers = new List<PatternUst>(),
                                            Name = new PatternIdRegexToken(@"\w+"),
                                            Body = new PatternReturnStatement(new PatternNullLiteral())
                                        },
                                        new PatternMethodDeclaration
                                        {
                                            Modifiers = new List<PatternUst>(),
                                            Name = new PatternIdRegexToken(@"\w+"),
                                            Body = new PatternArbitraryDepth
                                            {
                                                Pattern = new PatternThrowStatement
                                                (
                                                    new PatternObjectCreateExpression
                                                    {
                                                        Type = new PatternIdToken("UnsupportedOperationException"),
                                                        Arguments = new PatternArgs(new List<PatternUst>
                                                        {
                                                            new PatternStringRegexLiteral(".*")
                                                        })
                                                    }
                                                )
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ImproperValidationEmptyMethodPartial. Improper Certificate Validation (Empty method)",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternClassDeclaration
                {
                    BaseTypes = new List<PatternUst>
                    {
                        new PatternIdRegexToken("X509TrustManager|SSLSocketFactory")
                    },
                    Body = new PatternArbitraryDepth
                    {
                        Pattern = new PatternMethodDeclaration
                        {
                            Modifiers = new List<PatternUst>(),
                            Name = new PatternIdRegexToken(@"\w+"),
                            Body = new PatternReturnStatement(new PatternNullLiteral())
                        }
                    }
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PoorLoggingPractice. Declare logger not static or final",
                Languages = new HashSet<Language>() { Java.Language },
                Node = new PatternAnd
                (
                    new PatternNot
                    (
                        new PatternVarOrFieldDeclaration
                        {
                            LocalVariable = false,
                            Modifiers = new List<PatternUst>
                            {
                                new PatternIdToken("static"),
                                new PatternIdToken("final")
                            },
                            Type = new PatternIdRegexToken("[Ll]og"),
                            Assignment = new PatternAssignmentExpression(
                                new PatternIdRegexToken(),
                                null
                            )
                        }
                    ),
                    new PatternVarOrFieldDeclaration
                    {
                        LocalVariable = false,
                        Modifiers = new List<PatternUst>(),
                        Type = new PatternIdRegexToken("[Ll]og"),
                        Assignment = new PatternAssignmentExpression(
                            new PatternIdRegexToken(),
                            null
                        )
                    }
                )
            });

            return patterns;
        }
    }
}
