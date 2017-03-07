using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.Patterns.PatternsRepository
{
    public interface IPatternsRepository : ILoggable
    {
        string Path { get; set; }

        PatternDto GetByName(string name);

        IEnumerable<PatternDto> GetAll();

        void Add(IEnumerable<PatternDto> pattern);

        void Clear();
    }
}
