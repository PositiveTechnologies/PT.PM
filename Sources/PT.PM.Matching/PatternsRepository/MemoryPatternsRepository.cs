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

        public IEnumerable<string> Identifiers { get; set; } = Enumerable.Empty<string>();

        public MemoryPatternsRepository()
        {
        }

        public IEnumerable<PatternDto> GetAll()
        {
            if (PatternDtos == null)
            {
                PatternDtos = InitPatterns();
            }

            if (Identifiers.Count() > 0)
            {
                PatternDtos = PatternDtos.Where(dto => Identifiers.Contains(dto.Key)).ToList();
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
