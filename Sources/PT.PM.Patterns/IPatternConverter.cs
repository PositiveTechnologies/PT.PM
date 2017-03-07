using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Patterns
{
    public interface IPatternConverter<out TPatternDataStructure> : ILoggable
        where TPatternDataStructure : CommonPatternsDataStructure
    {
        Dictionary<UstNodeSerializationFormat, IAstNodeSerializer> AstNodeSerializers { get; set; }

        UstNodeSerializationFormat ConvertBackFormat { get; set; }

        TPatternDataStructure Convert(IEnumerable<PatternDto> patternsDto);

        IEnumerable<PatternDto> ConvertBack(CommonPatternsDataStructure dataStructure);
    }
}
