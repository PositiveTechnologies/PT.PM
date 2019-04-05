using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternAttributable
    {
        List<PatternUst> Attributes { get; set; }
    }
}
