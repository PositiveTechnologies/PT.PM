using System;
using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Patterns;

namespace PT.PM.Matching
{
    public interface IUstPatternMatcher<TPattern, TMatchingResult> : ILoggable
        where TPattern : PatternBase
        where TMatchingResult : MatchingResultBase<TPattern>
    {
        TPattern[] Patterns { get; set; }

        List<TMatchingResult> Match(Ust ust);
    }
}
