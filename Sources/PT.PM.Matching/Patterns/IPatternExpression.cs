using System;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternExpression
    {
        PatternUst[] GetArgs();

        [JsonIgnore]
        Type UstType { get; }
    }
}
