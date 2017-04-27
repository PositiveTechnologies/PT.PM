using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<Pattern> CreateHtmlPatterns()
        {
            var patterns = new List<Pattern>();

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 1 possible XSS",
                Languages = LanguageFlags.Html,
                FileNameWildcard = "**/app/views/*.html",
                Data = new PatternNode
                {
                    Node = new PatternStringLiteral("&{\\w+}")
                }
            });

            patterns.Add(new Pattern
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 2 possible XSS",
                Languages = LanguageFlags.Html,
                FileNameWildcard = "**/app/views/*.html",
                Data = new PatternNode
                {
                    Node = new PatternStringLiteral("@Html\\(\\w+\\)")
                }
            });

            return patterns;
        }
    }
}
