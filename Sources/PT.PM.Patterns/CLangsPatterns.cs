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
                Key = "Missing format string argument",
                DebugInfo = "Format string lack pattern",
                Languages = new HashSet<Language>() { ObjectiveC.Language, C.Language, CPlusPlus.Language },
                Node = new PatternOr (
                    new PatternInvocationExpression
                    {
                        Target = new PatternIdRegexToken("^(printf|NSLog|stringByAppendingFormat)$"),
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
                Key = "Hardcoded key for AES",
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
                Key = "Potential WebView LFI",
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
                Key = "Potential XXE",
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
                Key = "Potential SSL flaws",
                DebugInfo = "SSL configuration flaws(now it's not possible to figure out with promiscuous set of args, so match by property name",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node =  new PatternIdRegexToken("^(kCFStreamSSLPeerName|AFSSLPinningModeNone|setAllowInvalidCertificates|kCFStreamSSLAllowsAnyRoot|kCFStreamSSLValidatesCertificateChain|kCFStreamSSLAllowsExpiredCertificates|kCFStreamSSLAllowsExpiredRoots|setAllowsAnyHTTPSCertificate|continueWithoutCredentialForAuthenticationChallenge)$")
            });

            patterns.Add(new PatternRoot
            {
                Key = "Potential insecure file storing",
                DebugInfo = "Insecure file storing",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("^(kSecAttrAccessibleAlways|kSecAttrAccessibleWhenUnlocked|kSecAttrAccessibleAfterFirstUnlock)$")
            });

            patterns.Add(new PatternRoot
            {
                Key = "Potential insecure intreprocess mechanism",
                DebugInfo = "Deprecated and potentially insecure intreprocess mechanism",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("handleOpenURL")
            });

            patterns.Add(new PatternRoot
            {
                Key = "Potential JavaScript RCE in WebView",
                DebugInfo = "Potential JavaScript RCE in WebView",
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

            patterns.Add(new PatternRoot
            {
                Key = "Weak Crypto API",
                DebugInfo = "Deprecated cipher algorithms",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("^(kCCAlgorithmDES|kCCAlgorithm3DES|kCCAlgorithmRC2|kCCAlgorithmRC4)$")             
            });

            patterns.Add(new PatternRoot
            {
                Key = "Weak Hash API",
                DebugInfo = "Deprecated hash algorithms",
                Languages = new HashSet<Language>() { ObjectiveC.Language },
                Node = new PatternIdRegexToken("^(" +
                            "CC_MD2_Init|CC_MD2_Update|CC_MD2_Final|CC_MD2|MD2_Init|MD2_Update|MD2_Final|CC_MD4_Init|" +
                            "CC_MD4_Update|CC_MD4_Final|CC_MD4|MD4_Init|MD4_Update|MD4_Final|CC_MD5_Init|CC_MD5_Update|" +
                            "CC_MD5_Final|CC_MD5|MD5_Init|MD5_Update|MD5_Final|MD5Init|MD5Update|MD5Final|CC_SHA1_Init|" +
                            "CC_SHA1_Update|CC_SHA1_Final|CC_SHA1|SHA1_Init|SHA1_Update|SHA1_Final|CC_SHA224_Init|" +
                            "CC_SHA224_Update|CC_SHA224_Final|CC_SHA224|SHA224_Init|SHA224_Update|SHA224_Final|CC_SHA256_Init|" +
                            "CC_SHA256_Update|CC_SHA256_Final|CC_SHA256|SHA256_Init|SHA256_Update|SHA256_Final|CC_SHA384_Init|" +
                            "CC_SHA384_Update|CC_SHA384_Final|CC_SHA384|SHA384_Init|SHA384_Update|SHA384_Final|CC_SHA512_Init|" +
                            "CC_SHA512_Update|CC_SHA512_Final|CC_SHA512|SHA512_Init|SHA512_Update|SHA512_Final)$")
            });

            return patterns;
        }
    }
}
