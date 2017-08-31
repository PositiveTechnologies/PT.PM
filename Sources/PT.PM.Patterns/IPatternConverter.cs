using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Patterns
{
    public interface IPatternConverter<TPattern> : ILoggable
        where TPattern : PatternRootNode
    {
        Dictionary<UstNodeSerializationFormat, IUstNodeSerializer> UstNodeSerializers { get; set; }

        TPattern[] Convert(IEnumerable<PatternDto> patternDtos);

        PatternDto[] ConvertBack(IEnumerable<TPattern> patterns);
    }
}
