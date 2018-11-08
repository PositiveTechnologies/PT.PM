using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using PT.PM.SqlParseTreeUst;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateMySqlPatterns()
        {
            var patterns = new List<PatternRoot>();
            var mysql = MySql.Language;

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Weak Cryptographic Hash (MD2, MD4, MD5, RIPEMD-160, and SHA-1)",
                Node = new PatternInvocationExpression
                {
                    Target = new PatternIdRegexToken("sha1"),
                    Arguments = new PatternArgs(new PatternAny())
                }
            });
            return patterns;
        }
    }
}
