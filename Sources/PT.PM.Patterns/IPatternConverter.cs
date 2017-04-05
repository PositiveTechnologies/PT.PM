using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Patterns
{
    public interface IPatternConverter<TPattern> : ILoggable
        where TPattern : PatternBase
    {
        Dictionary<UstNodeSerializationFormat, IUstNodeSerializer> UstNodeSerializers { get; set; }

        UstNodeSerializationFormat ConvertBackFormat { get; set; }

        TPattern[] Convert(IEnumerable<PatternDto> patternDtos);

        PatternDto[] ConvertBack(IEnumerable<TPattern> patterns);
    }
}
