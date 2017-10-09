using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

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
            List<PatternDto> patternDtos = JsonConvert
                .DeserializeObject<List<PatternDto>>(patternsData, stringEnumConverter);

            var result = new List<PatternDto>();
            foreach (var patternDto in patternDtos)
            {
                result.Add(patternDto);
            }

            return result;
        }
    }
}
