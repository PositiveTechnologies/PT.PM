using System;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternExpression
    {
        PatternUst[] GetArgs();

        Type UstType { get; }
    }
}
