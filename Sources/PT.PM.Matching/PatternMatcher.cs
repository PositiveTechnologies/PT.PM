using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class PatternMatcher : IUstPatternMatcher<RootUst, PatternRootUst, MatchingResult>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public IEnumerable<PatternRootUst> Patterns { get; set; }

        public bool IsIgnoreFilenameWildcards { get; set; }

        public PatternMatcher(IEnumerable<PatternRootUst> patterns)
        {
            Patterns = patterns;
        }

        public PatternMatcher()
        {
        }

        public List<MatchingResult> Match(RootUst ust)
        {
            try
            {
                IEnumerable<PatternRootUst> patterns = Patterns
                    .Where(pattern => pattern.Languages.Any(patternLang => ust.Sublanguages.Contains(patternLang)));
                if (!IsIgnoreFilenameWildcards)
                {
                    patterns = patterns.Where(pattern => pattern.FilenameWildcardRegex?.IsMatch(ust.SourceCodeFile.FullPath) ?? true);
                }

                var result = new List<MatchingResult>();
                foreach (PatternRootUst pattern in patterns)
                {
                    var context = new MatchingContext(pattern);
                    pattern.Match(ust, context);
                    result.AddRange(context.Results);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new MatchingException(ust.SourceCodeFile.FullPath, ex));
                return new List<MatchingResult>();
            }
        }
    }
}
