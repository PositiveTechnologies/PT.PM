using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Matching
{
    public interface IUstPatternMatcher<TInputGraph, TPattern, TMatchResult> : ILoggable
        where TMatchResult : MatchResultBase<TPattern>
    {
        bool IsIgnoreFilenameWildcards { get; set; }

        List<TPattern> Patterns { get; set; }

        void Match(TInputGraph ust);
    }
}
