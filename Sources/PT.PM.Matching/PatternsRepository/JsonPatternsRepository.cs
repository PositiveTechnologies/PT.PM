using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.PatternsRepository
{
    public class JsonPatternsRepository : MemoryPatternsRepository
    {
        private string patternsData;
        private readonly JsonConverter stringEnumConverter;

        public JsonPatternsRepository(string patternsData)
        {
            this.patternsData = patternsData;
            stringEnumConverter = new StringEnumConverter();
        }

        protected override List<PatternDto> InitPatterns()
        {
            var languageFlagsConverter = new PatternJsonSafeConverter
            {
                Logger = Logger
            };

            List<PatternDto> patternDtos = JsonConvert
                .DeserializeObject<List<PatternDto>>(patternsData, stringEnumConverter, languageFlagsConverter);

            var result = new List<PatternDto>();
            foreach (var patternDto in patternDtos)
            {
                if (patternDto.Languages.Count() == 0)
                {
                    Logger.LogInfo($"PatternNode \"{patternDto.Key}\" ignored because of it doesn't have target languages.");
                }
                else
                {
                    result.Add(patternDto);
                }
            }

            return result;
        }
    }
}
