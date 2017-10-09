using PT.PM.Matching.PatternsRepository;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public class DslPatternRepository : MemoryPatternsRepository
    {
        private string patternData;
        private HashSet<string> languages;

        public DslPatternRepository(string patternData, string language)
            : this(patternData, new [] { language })
        {
        }

        public DslPatternRepository(string patternData, IEnumerable<string> languages)
        {
            this.patternData = patternData;
            this.languages = new HashSet<string>(languages);
        }

        protected override List<PatternDto> InitPatterns()
        {
            return new List<PatternDto>()
            {
                new PatternDto
                {
                    Key = "temp",
                    Languages = languages,
                    DataFormat = "Dsl",
                    Value = patternData
                }
            };
        }
    }
}
