using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Patterns
{
    public class CommonPatternsDataStructure
    {
        public Pattern[] Patterns { get; set; }

        public CommonPatternsDataStructure(IEnumerable<Pattern> patterns)
        {
            Patterns = patterns.ToArray();
        }
    }
}
