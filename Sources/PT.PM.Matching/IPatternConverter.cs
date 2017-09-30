using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Matching
{
    public interface IPatternConverter<TPattern> : ILoggable
    {
        Dictionary<UstSerializeFormat, IUstSerializer> UstNodeSerializers { get; set; }

        TPattern[] Convert(IEnumerable<PatternDto> patternDtos);

        PatternDto[] ConvertBack(IEnumerable<TPattern> patterns);
    }
}
