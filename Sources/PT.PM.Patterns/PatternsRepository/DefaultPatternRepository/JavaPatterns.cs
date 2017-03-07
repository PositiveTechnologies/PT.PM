using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<Pattern> CreateJavaPatterns()
        {
            var patterns = new List<Pattern>();

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InadequateRsaPadding. Weak Encryption: Inadequate RSA Padding. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/weak_encryption_inadequate_rsa_padding.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("getInstance"),
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken("Cipher"),
                                Target = new MemberReferenceExpression
                                {
                                    Name = new IdToken("crypto"),
                                    Target = new IdToken("javax")
                                }
                            }
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternStringLiteral("^RSA/NONE/NoPadding$") })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicAlgorithm. Weak Encryption: Broken or Risky Cryptographic Algorithm" +
                    "https://cwe.mitre.org/data/definitions/327.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("getInstance"),
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken("Cipher"),
                                Target = new MemberReferenceExpression
                                {
                                    Name = new IdToken("crypto"),
                                    Target = new IdToken("javax")
                                }
                            }
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternStringLiteral(@"DES") })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadPath. Cookie Security: Overly Broad Path: " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/cookie_security_overly_broad_path.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("setPath"),
                            Target = new PatternIdToken(@"[cC]ookie")
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternStringLiteral { Text = "^/?$" } })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadDomain Cookie Security: Overly Broad Domain: " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/cookie_security_overly_broad_domain.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("setDomain"),
                            Target = new PatternIdToken { Id = @"[cC]ookie" }
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternStringLiteral(@"^.?[a-zA-Z0-9\-]+\.[a-zA-Z0-9\-]+$") })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PoorSeeding. Poor Seeding: " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/insecure_randomness_poor_seed.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("setSeed"),
                            Target = new PatternExpression()
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternIntLiteral() })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash. Weak Cryptographic Hash: " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/weak_cryptographic_hash.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("getInstance"),
                            Target = new IdToken("MessageDigest")
                        },
                        Arguments = new ArgsNode(new Expression[] { new PatternStringLiteral("MD5|SHA-1") })
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidPermissionCheck. Often Misused: Android Permission Check. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/often_misused_android_permission_check.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Name = new PatternIdToken("^(checkCallingOrSelfPermission|checkCallingOrSelfUriPermission)$"),
                            Target = new PatternExpression()
                        },
                        Arguments = new PatternExpressions()
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidHostnameVerificationDisabled. Insecure SSL: Android Hostname Verification Disabled. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/insecure_ssl_android_hostname_verification_disabled.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new PatternVarDef
                    {
                        Values = new Expression[]
                    {
                        new MemberReferenceExpression
                        {
                            Name = new IdToken("ALLOW_ALL_HOSTNAME_VERIFIER"),
                            Target = new IdToken("SSLSocketFactory")
                        },
                        new ObjectCreateExpression
                        {
                            Type = new TypeToken { TypeText = "AllowAllHostnameVerifier" },
                             Arguments = new PatternExpressions()
                        }
                    }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SAXReaderExternalEntity",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                        Collection = new Expression[]
                        {
                            new PatternExpression(new PatternStringLiteral(), true)
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("read"),
                            Target = new ObjectCreateExpression
                            {
                                Type = new TypeToken { TypeText = "SAXReader" },
                                Arguments = new ArgsNode()
                            }
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "XmlExternalEntity",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[]
                        {
                            new PatternExpression(new PatternStringLiteral(), true)
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("parse"),
                            Target = new ObjectCreateExpression
                            {
                                Type = new TypeToken { TypeText = "XMLUtil" },
                                Arguments = new ArgsNode()
                            }
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "StickyBroadcast. Android Bad Practices: Sticky Broadcast. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/android_bad_practices_sticky_broadcast.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[] { new PatternExpression() }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("sendStickyBroadcast"),
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SendStickyBroadcastAsUser. Android Bad Practices: Sticky Broadcast. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/android_bad_practices_sticky_broadcast.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[] { new PatternExpression(), new PatternExpression() }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken("sendStickyBroadcastAsUser"),
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            // TODO: implement "createSocket"
            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureSSL. Insecure SSL: Android Socket. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/insecure_ssl_android_socket.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[]
                        {
                            new PatternExpression(),
                            new PatternExpression()
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken { Id = "getInsecure" },
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedSalt. Weak Cryptographic Hash: Hardcoded Salt. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/weak_cryptographic_hash_hardcoded_salt.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[] {
                            new PatternExpression(),
                            new PatternStringLiteral()
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken { Id = "hash" },
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingReceiverPermission. The program sends a broadcast without specifying the receiver permission. " +
                              "Broadcasts sent without the receiver permission are accessible to any receiver. If these broadcasts contain sensitive data or reach a malicious receiver, the application may be compromised. " +
                              "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/android_bad_practices_missing_receiver_permission.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[] {
                            new PatternExpression()
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken { Id = "sendBroadcast" },
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingBroadcasterPermission. The program registers a receiver without specifying the broadcaster permission. " +
                    "Receiver registered without the broadcaster permission will receive messages from any broadcaster. " +
                    "If these messages contain malicious data or come from a malicious broadcaster, the application may be compromised. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/android_bad_practices_missing_broadcaster_permission.html " +
                    "Use this form: public abstract Intent registerReceiver (BroadcastReceiver receiver, IntentFilter filter, String broadcastPermission, Handler scheduler)",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Node = new InvocationExpression
                    {
                        Arguments = new ArgsNode
                        {
                            Collection = new Expression[] {
                            new PatternExpression(),
                            new PatternExpression()
                        }
                        },
                        Target = new MemberReferenceExpression
                        {
                            Name = new IdToken { Id = "registerReceiver" },
                            Target = new PatternExpression()
                        }
                    }
                }
            });

            var cookieVar = new PatternVarDef
            {
                Id = "cookie",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieNotSentOverSSL. Cookie Security: Cookie not Sent Over SSL. " +
                    "http://www.hpenterprisesecurity.com/vulncat/en/vulncat/java/cookie_security_cookie_not_sent_over_ssl.html",
                Languages = LanguageFlags.Java,
                Data = new PatternNode
                {
                    Vars = new List<PatternVarDef> { cookieVar },
                    Node = new PatternStatements
                    {
                        Statements = new Statement[]
                    {
                        new ExpressionStatement(new VariableDeclarationExpression
                        {
                            Type = new TypeToken() { TypeText = "Cookie" },
                            Variables = new AssignmentExpression[]
                            {
                                new AssignmentExpression
                                {
                                    Left = new PatternVarRef(cookieVar),
                                    Right = new ObjectCreateExpression
                                    {
                                        Type = new TypeToken { TypeText = "Cookie" },
                                        Arguments = new PatternExpressions()
                                    },
                                }
                            }
                        }),

                        new PatternMultipleStatements(),

                        new PatternStatement(new ExpressionStatement(new InvocationExpression
                        {
                            Arguments = new ArgsNode
                            {
                                Collection = new Expression[] { new BooleanLiteral { Value = true } }
                            },
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken { Id = "setSecure" },
                                Target = new PatternVarRef(cookieVar)
                            }
                        }), true),

                        new PatternMultipleStatements(),

                        new ExpressionStatement(new InvocationExpression
                        {
                            Arguments = new ArgsNode
                            {
                                Collection = new Expression[] { new PatternVarRef(cookieVar) }
                            },
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken { Id = "addCookie" },
                                Target = new PatternExpression()
                            }
                        })
                    }
                    }
                }
            });

            return patterns;
        }
    }
}
