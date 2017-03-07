using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;
using System;

namespace PT.PM.Patterns
{
    public class CommonPatternConverter : IPatternConverter<CommonPatternsDataStructure>
    {
        private ILogger logger { get; set; } = DummyLogger.Instance;

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
                foreach (var serializer in AstNodeSerializers)
                {
                    serializer.Value.Logger = logger;
                }
            }
        }
        public Dictionary<UstNodeSerializationFormat, IAstNodeSerializer> AstNodeSerializers { get; set; }

        public UstNodeSerializationFormat ConvertBackFormat { get; set; }

        public CommonPatternConverter(IAstNodeSerializer serializer, UstNodeSerializationFormat format = UstNodeSerializationFormat.Json)
            : this(new[] { serializer }, format)
        {
        }

        public CommonPatternConverter(IEnumerable<IAstNodeSerializer> serializers, UstNodeSerializationFormat format = UstNodeSerializationFormat.Json)
        {
            AstNodeSerializers = new Dictionary<UstNodeSerializationFormat, IAstNodeSerializer>();
            foreach (var serializer in serializers)
            {
                AstNodeSerializers[serializer.DataFormat] = serializer;
            }
            ConvertBackFormat = format;
        }

        public CommonPatternsDataStructure Convert(IEnumerable<PatternDto> patternsDto)
        {
            var result = new List<Pattern>();
            foreach (var patternDto in patternsDto)
            {
                var serializer = AstNodeSerializers[patternDto.DataFormat];
                try
                {
                    PatternNode data = (PatternNode)serializer.Deserialize(patternDto.Value, patternDto.Languages);
                    var pattern = new Pattern(patternDto, data);
                    result.Add(pattern);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error while \"{patternDto.Key}\" pattern deserialising ({patternDto.Value}) ", ex);
                }
            }
            return new CommonPatternsDataStructure(result);
        }

        public IEnumerable<PatternDto> ConvertBack(CommonPatternsDataStructure dataStructure)
        {
            var result = new List<PatternDto>();
            foreach (var pattern in dataStructure.Patterns)
            {
                var serializer = AstNodeSerializers[ConvertBackFormat];
                try
                {
                    var serializedData = serializer.Serialize(pattern.Data);
                    result.Add(new PatternDto(pattern, serializer.DataFormat, serializedData));
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error while \"{pattern.Key}\" pattern serialising", ex);
                }
            }
            return result;
        }
    }
}
