using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;
using static PT.PM.Common.Language;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRootNode> CreateHtmlPatterns()
        {
            var patterns = new List<PatternRootNode>();

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 1 possible XSS",
                Languages = new HashSet<Language>() { Html },
                FilenameWildcard = "**/app/views/*.html",
                Node = new PatternStringLiteral("&{\\w+}")
            });

            patterns.Add(new PatternRootNode
            {
                Key = patternIdGenerator.NextId(),
                DebugInfo = "Play 2 possible XSS",
                Languages = new HashSet<Language>() { Html },
                FilenameWildcard = "**/app/views/*.html",
                Node = new PatternStringLiteral("@Html\\(\\w+\\)")
            });

            return patterns;
        }
    }
}
