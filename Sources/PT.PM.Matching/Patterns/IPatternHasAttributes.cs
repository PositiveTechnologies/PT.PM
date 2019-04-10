using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternHasAttributes
    {
        List<PatternUst> Attributes { get; set; }
    }
}
