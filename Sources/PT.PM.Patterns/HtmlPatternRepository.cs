using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateHtmlPatterns()
        {
            var patterns = new List<PatternRoot>();

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 1 possible XSS",
                Languages = new HashSet<Language> { Language.Html },
                FilenameWildcard = "**/app/views/*.html",
                Node = new PatternStringRegexLiteral("&{\\w+}")
            });

            patterns.Add(new PatternRoot
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 2 possible XSS",
                Languages = new HashSet<Language> { Language.Html },
                FilenameWildcard = "**/app/views/*.html",
                Node = new PatternStringRegexLiteral("@Html\\(\\w+\\)")
            });

            return patterns;
        }
    }
}
