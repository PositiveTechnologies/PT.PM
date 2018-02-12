using System.Collections.Generic;

namespace PT.PM.Matching.PatternsRepository
{
    public class CombiningPatternsRepository : MemoryPatternsRepository
    {
        private IPatternsRepository[] patternsRepositories;

        public CombiningPatternsRepository(params IPatternsRepository[] patternsRepositories)
        {
            this.patternsRepositories = patternsRepositories;
        }

        protected override List<PatternDto> InitPatterns()
        {
            var result = new List<PatternDto>();
            foreach (IPatternsRepository patternRepository in patternsRepositories)
            {
                result.AddRange(patternRepository.GetAll());
            }
            return result;
        }
    }
}
