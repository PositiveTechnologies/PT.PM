using PT.PM.Patterns.PatternsRepository;
using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Patterns
{
    public class DslPatternRepository : MemoryPatternsRepository
    {
        private string patternData;
        private LanguageFlags languageFlags;

        public DslPatternRepository(string patternData, LanguageFlags languageFlags)
        {
            this.patternData = patternData;
            this.languageFlags = languageFlags;
        }

        protected override List<PatternDto> InitPatterns()
        {
            return new List<PatternDto>()
            {
                new PatternDto
                {
                    Key = "temp",
                    Languages = languageFlags,
                    DataFormat = UstNodeSerializationFormat.Dsl,
                    Value = patternData
                }
            };
        }
    }
}
