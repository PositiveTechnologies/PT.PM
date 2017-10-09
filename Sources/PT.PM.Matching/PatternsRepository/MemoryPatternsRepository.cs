using System.Collections.Generic;
using System.Linq;
using PT.PM.Common;

namespace PT.PM.Matching.PatternsRepository
{
    public class MemoryPatternsRepository : IPatternsRepository
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string Path { get; set; }

        protected List<PatternDto> PatternDtos { get; set; }

        public MemoryPatternsRepository()
        {
        }

        public IEnumerable<PatternDto> GetAll()
        {
            if (PatternDtos == null)
            {
                PatternDtos = InitPatterns();
            }

            return PatternDtos;
        }

        public void Add(IEnumerable<PatternDto> pattern)
        {
            if (PatternDtos == null)
            {
                PatternDtos = InitPatterns();
            }

            PatternDtos.AddRange(pattern);
        }

        public PatternDto GetByName(string name)
        {
            if (PatternDtos == null)
            {
                PatternDtos = InitPatterns();
            }

            return PatternDtos.FirstOrDefault(p => p.Description == name);
        }

        public void Clear()
        {
            PatternDtos.Clear();
        }

        protected virtual List<PatternDto> InitPatterns()
        {
            return new List<PatternDto>();
        }
    }
}
