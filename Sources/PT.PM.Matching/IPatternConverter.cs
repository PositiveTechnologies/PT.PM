using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Matching
{
    public interface IPatternConverter<TPattern> : ILoggable
    {
        List<IPatternSerializer> Serializers { get; set; }

        List<TPattern> Convert(IEnumerable<PatternDto> patternDtos);

        List<PatternDto> ConvertBack(IEnumerable<TPattern> patterns);
    }
}
