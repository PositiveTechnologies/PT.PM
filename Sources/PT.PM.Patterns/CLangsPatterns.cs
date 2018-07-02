using PT.PM.CLangsParseTreeUst;
using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateCLangsPatterns()
        {
          
            var patterns = new List<PatternRoot>();
            
            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Format string lack pattern",
                Languages = new HashSet<Language>() { ObjectiveC.Language, C.Language, CPlusPlus.Language },
                Node = new PatternOr (
                    new PatternInvocationExpression
                    {
                        Target = new PatternIdRegexToken("^(syslog|printf|NSLog|stringByAppendingFormat)$"),
                        Arguments = new PatternArgs(new PatternNot(new PatternStringRegexLiteral(".*")))
                    },
                    new PatternInvocationExpression
                    {
                        Target = new PatternIdRegexToken("^((s|f)printf)$"),
                        Arguments = new PatternArgs(new PatternNot(new PatternStringRegexLiteral(".*")), new PatternAnyExpression())
                    },
                    new PatternInvocationExpression
                    {
                        Target = new PatternIdRegexToken("^(snprintf)$"),
                        Arguments = new PatternArgs(new PatternNot(new PatternStringRegexLiteral(".*")), new PatternAnyExpression(), new PatternAnyExpression())
                    }
                )    
            });
            
            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Hardcoded key for AES",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternOr(new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("encryptData:withSettings:password:error:"),
                        Target = new PatternIdRegexToken("RN(?i)Encrypt")
                    },
                    Arguments = new PatternArgs(new PatternIdRegexToken(".*"), new PatternIdRegexToken("kRNCrypto"), new PatternStringRegexLiteral(@".*"), new PatternAnyExpression())
                },
                new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("decryptData:withPassword:error:"),
                        Target = new PatternIdRegexToken("RN(?i)Decrypt")
                    },
                    Arguments = new PatternArgs(new PatternIdRegexToken(".*"), new PatternStringRegexLiteral(@".*"), new PatternAnyExpression())
                })
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
  
                DebugInfo = "WebView LFI",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("loadHTMLString:baseURL:"),
                        Target = new PatternMemberReferenceExpression
                        {
                            Name = new PatternIdRegexToken("(?i)webView"),
                            Target = new PatternIdRegexToken(".*")
                        }
                    },
                    Arguments = new PatternArgs(new PatternAnyExpression(), new PatternIntLiteral(0))
               }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "XXE if setShouldResolveExternalEntities is enabled",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("setShouldResolveExternalEntities"),
                        Target = new PatternIdRegexToken("addressParser")
                    },
                    Arguments = new PatternArgs(new PatternIntLiteral(1))
                }
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "SSL configuration flaws(now it's not possible to figure out with promiscuous set of args, so match by property name",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node =  new PatternIdRegexToken("^(kCFStreamSSLPeerName|AFSSLPinningModeNone|setAllowInvalidCertificates|kCFStreamSSLAllowsAnyRoot|kCFStreamSSLValidatesCertificateChain|kCFStreamSSLAllowsExpiredCertificates|kCFStreamSSLAllowsExpiredRoots|setAllowsAnyHTTPSCertificate|continueWithoutCredentialForAuthenticationChallenge)$")
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Insecure file storing",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("^(kSecAttrAccessibleAlways|kSecAttrAccessibleWhenUnlocked|kSecAttrAccessibleAfterFirstUnlock)$")
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Deprecated and potentially insecure intreprocess mechanism",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("handleOpenURL")
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Potentially JavaScript RCE in WebView",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternInvocationExpression
                {
                    Target = new PatternMemberReferenceExpression
                    {
                        Name = new PatternIdRegexToken("stringByEvaluatingJavaScriptFromString"),
                        Target = new PatternIdRegexToken("(?i)webView")
                    },
                    Arguments = new PatternArgs(new PatternOr (new PatternNot(new PatternStringRegexLiteral(".*"))), new PatternStringRegexLiteral("exec"))
                }
            });

            return patterns;
        }
    }
}
