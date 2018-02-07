using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.Matching.PatternsRepository
{
    public interface IPatternsRepository : ILoggable
    {
        string Path { get; set; }

        IEnumerable<string> Identifiers { get; set; }

        IEnumerable<PatternDto> GetAll();

        void Add(IEnumerable<PatternDto> pattern);

        void Clear();
    }
}
