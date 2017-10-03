using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Matching.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class PatternConverter : IPatternConverter<PatternRoot>
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
                foreach (IPatternSerializer serializer in Serializers)
                {
                    serializer.Logger = logger;
                }
            }
        }

        public List<IPatternSerializer> Serializers { get; set; }

        public PatternConverter()
        {
            Serializers = new List<IPatternSerializer>
            {
                new JsonPatternSerializer()
            };
        }

        public PatternConverter(params IPatternSerializer[] serializers)
        {
            Serializers = serializers.ToList();
        }

        public PatternConverter(IEnumerable<IPatternSerializer> serializers)
        {
            Serializers = serializers.ToList();
        }

        public PatternRoot[] Convert(IEnumerable<PatternDto> patternsDto)
        {
            var result = new List<PatternRoot>(patternsDto.Count());
            foreach (PatternDto patternDto in patternsDto)
            {
                IPatternSerializer serializer = Serializers.FirstOrDefault(s => s.Format == patternDto.DataFormat)
                    ?? Serializers.First();
                if (serializer == null)
                {
                    Logger.LogError(new ConversionException("", null, $"Serializer for {patternDto.DataFormat} has not been found", true));
                    continue;
                }

                try
                {
                    PatternRoot pattern = serializer.Deserialize(patternDto.Value);

                    pattern.DataFormat = serializer.Format;
                    pattern.Key = patternDto.Key;
                    pattern.Languages = patternDto.Languages;
                    pattern.DebugInfo = patternDto.Description;
                    pattern.FilenameWildcard = patternDto.FilenameWildcard;

                    result.Add(pattern);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException("", ex, $"Error while \"{patternDto.Key}\" pattern deserialising ({patternDto.Value}) ", true));
                }
            }
            return result.ToArray();
        }

        public PatternDto[] ConvertBack(IEnumerable<PatternRoot> patterns)
        {
            var result = new List<PatternDto>();
            foreach (PatternRoot pattern in patterns)
            {
                IPatternSerializer serializer = Serializers.FirstOrDefault(s => s.Format == pattern.DataFormat)
                    ?? Serializers.First();
                try
                {
                    string serialized = serializer.Serialize(pattern);
                    PatternDto patternDto = new PatternDto
                    {
                        DataFormat = pattern.DataFormat,
                        Key = pattern.Key,
                        Languages = pattern.Languages,
                        Value = serialized,
                        Description = pattern.DebugInfo,
                        FilenameWildcard = pattern.FilenameWildcard
                    };
                    result.Add(patternDto);
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
