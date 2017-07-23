using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;
using System;
using PT.PM.Common.Exceptions;

namespace PT.PM.Patterns
{
    public class PatternConverter : IPatternConverter<Pattern>
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
                foreach (var serializer in UstNodeSerializers)
                {
                    serializer.Value.Logger = logger;
                }
            }
        }

        public Dictionary<UstNodeSerializationFormat, IUstNodeSerializer> UstNodeSerializers { get; set; }

        public UstNodeSerializationFormat ConvertBackFormat { get; set; }

        public PatternConverter(IUstNodeSerializer serializer, UstNodeSerializationFormat format = UstNodeSerializationFormat.Json)
            : this(new[] { serializer }, format)
        {
        }

        public PatternConverter(IEnumerable<IUstNodeSerializer> serializers, UstNodeSerializationFormat format = UstNodeSerializationFormat.Json)
        {
            UstNodeSerializers = new Dictionary<UstNodeSerializationFormat, IUstNodeSerializer>();
            foreach (var serializer in serializers)
            {
                UstNodeSerializers[serializer.DataFormat] = serializer;
            }
            ConvertBackFormat = format;
        }

        public Pattern[] Convert(IEnumerable<PatternDto> patternsDto)
        {
            var result = new List<Pattern>();
            foreach (var patternDto in patternsDto)
            {
                var serializer = UstNodeSerializers[patternDto.DataFormat];
                try
                {
                    PatternNode data = (PatternNode)serializer.Deserialize(patternDto.Value, patternDto.Languages);
                    var pattern = new Pattern(patternDto, data);
                    result.Add(pattern);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException("", ex, $"Error while \"{patternDto.Key}\" pattern deserialising ({patternDto.Value}) ", true));
                }
            }
            return result.ToArray();
        }

        public PatternDto[] ConvertBack(IEnumerable<Pattern> patterns)
        {
            var result = new List<PatternDto>();
            foreach (var pattern in patterns)
            {
                var serializer = UstNodeSerializers[ConvertBackFormat];
                try
                {
                    var serializedData = serializer.Serialize(pattern.Data);
                    result.Add(new PatternDto(pattern, serializer.DataFormat, serializedData));
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException("", ex, $"Error while \"{pattern.Key}\" pattern serialising", true));
                }
            }
            return result.ToArray();
        }
    }
}
