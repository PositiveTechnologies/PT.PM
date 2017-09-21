using PT.PM.Common;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes;
using System;
using System.Linq;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootUst> CreateJavaPatterns()
        {
            var patterns = new List<PatternRootUst>();

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InadequateRsaPadding. Weak Encryption: Inadequate RSA Padding. ",
                Languages = new HashSet<Language>() { Java },
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
                    Arguments = new ArgsUst(new List<Expression>() { new PatternStringLiteral("^RSA/NONE/NoPadding$") })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicAlgorithm. Weak Encryption: Broken or Risky Cryptographic Algorithm" +
                    "https://cwe.mitre.org/data/definitions/327.html",
                Languages = new HashSet<Language>() { Java },
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
                    Arguments = new ArgsUst(new List<Expression>() { new PatternStringLiteral(@"DES") })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadPath. Cookie Security: Overly Broad Path.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("setPath"),
                        Target = new PatternIdToken(@"[cC]ookie")
                    },
                    Arguments = new ArgsUst(new List<Expression>() { new PatternStringLiteral { Text = "^/?$" } })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "OverlyBroadDomain Cookie Security: Overly Broad Domain.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("setDomain"),
                        Target = new PatternIdToken { Id = @"[cC]ookie" }
                    },
                    Arguments = new ArgsUst(new List<Expression>() { new PatternStringLiteral(@"^.?[a-zA-Z0-9\-]+\.[a-zA-Z0-9\-]+$") })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PoorSeeding.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("setSeed"),
                        Target = new PatternExpression()
                    },
                    Arguments = new ArgsUst(new List<Expression>() { new PatternIntLiteral() })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "WeakCryptographicHash.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("getInstance"),
                        Target = new IdToken("MessageDigest")
                    },
                    Arguments = new ArgsUst(new List<Expression>() { new PatternStringLiteral("MD5|SHA-1") })
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidPermissionCheck. Often Misused: Android Permission Check.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Name = new PatternIdToken("^(checkCallingOrSelfPermission|checkCallingOrSelfUriPermission)$"),
                        Target = new PatternExpression()
                    },
                    Arguments = new PatternExpressions(new PatternMultipleExpressions())
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "AndroidHostnameVerificationDisabled. Insecure SSL: Android Hostname Verification Disabled.",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternVarDef
                {
                    Values = new List<Expression>()
                    {
                        new MemberReferenceExpression
                        {
                            Name = new IdToken("ALLOW_ALL_HOSTNAME_VERIFIER"),
                            Target = new IdToken("SSLSocketFactory")
                        },
                        new ObjectCreateExpression
                        {
                            Type = new TypeToken { TypeText = "AllowAllHostnameVerifier" },
                            Arguments = new PatternExpressions(new PatternMultipleExpressions())
                        }
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SAXReaderExternalEntity",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>()
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
                            Arguments = new ArgsUst()
                        }
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "XmlExternalEntity",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>()
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
                            Arguments = new ArgsUst()
                        }
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "StickyBroadcast. Android Bad Practices: Sticky Broadcast.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>() { new PatternExpression() }
                    },
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("sendStickyBroadcast"),
                        Target = new PatternExpression()
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SendStickyBroadcastAsUser. Android Bad Practices: Sticky Broadcast.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>() { new PatternExpression(), new PatternExpression() }
                    },
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken("sendStickyBroadcastAsUser"),
                        Target = new PatternExpression()
                    }
                }
            });

            // TODO: implement "createSocket"
            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "InsecureSSL. Insecure SSL: Android Socket.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>()
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
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "HardcodedSalt. Weak Cryptographic Hash: Hardcoded Salt.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>() {
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
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingReceiverPermission. The program sends a broadcast without specifying the receiver permission. " +
                              "Broadcasts sent without the receiver permission are accessible to any receiver. If these broadcasts contain sensitive data or reach a malicious receiver, the application may be compromised.",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>() {
                            new PatternExpression()
                        }
                    },
                    Target = new MemberReferenceExpression
                    {
                        Name = new IdToken { Id = "sendBroadcast" },
                        Target = new PatternExpression()
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "MissingBroadcasterPermission. The program registers a receiver without specifying the broadcaster permission. " +
                    "Receiver registered without the broadcaster permission will receive messages from any broadcaster. " +
                    "If these messages contain malicious data or come from a malicious broadcaster, the application may be compromised. " +
                    "Use this form: public abstract Intent registerReceiver (BroadcastReceiver receiver, IntentFilter filter, String broadcastPermission, Handler scheduler)",
                Languages = new HashSet<Language>() { Java },
                Node = new InvocationExpression
                {
                    Arguments = new ArgsUst
                    {
                        Collection = new List<Expression>() {
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
            });

            var cookieVar = new PatternVarDef
            {
                Id = "cookie",
                Values = new List<Expression>() { new PatternIdToken() }
            };
            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "CookieNotSentOverSSL. Cookie Security: Cookie not Sent Over SSL. ",
                Languages = new HashSet<Language>() { Java },
                Vars = new List<PatternVarDef> { cookieVar },
                Node = new PatternStatements
                {
                    Statements = new List<Statement>()
                    {
                        new ExpressionStatement(new VariableDeclarationExpression
                        {
                            Type = new TypeToken() { TypeText = "Cookie" },
                            Variables = new List<AssignmentExpression>
                            {
                                new AssignmentExpression
                                {
                                    Left = new PatternVarRef(cookieVar),
                                    Right = new ObjectCreateExpression
                                    {
                                        Type = new TypeToken { TypeText = "Cookie" },
                                        Arguments = new PatternExpressions(new PatternMultipleExpressions())
                                    },
                                }
                            }
                        }),

                        new PatternMultipleStatements(),

                        new PatternStatement(new ExpressionStatement(new InvocationExpression
                        {
                            Arguments = new ArgsUst
                            {
                                Collection = new List<Expression>() { new BooleanLiteral { Value = true } }
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
                            Arguments = new ArgsUst
                            {
                                Collection = new List<Expression>() { new PatternVarRef(cookieVar) }
                            },
                            Target = new MemberReferenceExpression
                            {
                                Name = new IdToken { Id = "addCookie" },
                                Target = new PatternExpression()
                            }
                        })
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Use of NullPointerException Catch to Detect NULL Pointer Dereference",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternTryCatchStatement
                {
                    ExceptionTypes = new List<Token> { new TypeToken("NullPointerException") },
                    IsCatchBodyEmpty = false
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "UsingCloneWithoutCloneable. Using clone method without implementing Clonable",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternAnd
                {
                    Expressions = new List<Expression>
                    {
                        new PatternClassDeclaration
                        {
                            Body = new PatternExpressionInsideNode
                            {
                                Expression = new PatternMethodDeclaration
                                {
                                    Name = new IdToken("clone"),
                                    AnyBody = true
                                }
                            }
                        },

                        new PatternNot
                        {
                            Expression = new PatternClassDeclaration
                            {
                                BaseTypes = new List<Token>{ new TypeToken("Cloneable") }
                            }
                        }
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ExtendingSecurityManagerWithoutFinal. Class extending SecurityManager is not final",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternAnd
                {
                    Expressions = new List<Expression>
                    {
                        new PatternClassDeclaration
                        {
                            BaseTypes = new List<Token>
                            {
                                new PatternIdToken("SecurityManager")
                            }
                        },

                        new PatternNot
                        {
                            Expression = new PatternClassDeclaration
                            {
                                Modifiers = new List<Token>
                                {
                                    new PatternIdToken("final")
                                }
                            }
                        }
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "ImproperValidationEmptyMethod. Improper Certificate Validation (Empty method)",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternClassDeclaration
                {
                    BaseTypes = new List<Token>
                    {
                        new PatternIdToken("X509TrustManager|SSLSocketFactory")
                    },
                    Body = new PatternExpressionInsideNode
                    {
                        Expression = new PatternMethodDeclaration(
                            Enumerable.Empty<Token>().ToList(), new PatternIdToken(".+"), false)
                    }
                }
            });

            patterns.Add(new PatternRootUst
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "PoorLoggingPractice. Declare logger not static or final",
                Languages = new HashSet<Language>() { Java },
                Node = new PatternAnd
                {
                    Expressions = new List<Expression>
                    {
                        new PatternVarOrFieldDeclaration
                        {
                            LocalVariable = false,
                            Modifiers = new List<Token>(),
                            Type = new PatternIdToken("[Ll]og"),
                            Name = new PatternIdToken(".+")
                        },
                        new PatternNot
                        {
                            Expression = new PatternVarOrFieldDeclaration
                            {
                                LocalVariable = false,
                                Modifiers = new List<Token>
                                {
                                    new PatternIdToken("static"),
                                    new PatternIdToken("final")
                                },
                                Type = new PatternIdToken("[Ll]og"),
                                Name = new PatternIdToken(".+")
                            }
                        }
                    }
                }
            });

            return patterns;
        }
    }
}
