using System;
using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Patterns;

namespace PT.PM.Matching
{
    public interface IAstPatternMatcher<TPatternsDataStructure> : ILoggable
        where TPatternsDataStructure : CommonPatternsDataStructure
    {
        TPatternsDataStructure PatternsData { get; set; }

        IEnumerable<MatchingResult> Match(Ust ast);
    }
}
