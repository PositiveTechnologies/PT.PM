using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Patterns
{
    public interface IPatternConverter<TPattern> : ILoggable
    {
        Dictionary<UstNodeSerializationFormat, IUstNodeSerializer> UstNodeSerializers { get; set; }

        TPattern[] Convert(IEnumerable<PatternDto> patternDtos);

        PatternDto[] ConvertBack(IEnumerable<TPattern> patterns);
    }
}
