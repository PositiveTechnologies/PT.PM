using PT.PM.CLangsParseTreeUst;
using PT.PM.Common;
using PT.PM.Matching;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Patterns.PatternsRepository
{
    public partial class DefaultPatternRepository
    {
        public IEnumerable<PatternRoot> CreateCLangsPatterns()
        {
            // Init dummy pattern for correct CLangsParseTreeUst assembly initialization.
            var result = new PatternRoot
            {
                Languages = new HashSet<Language>() { C.Language, CPlusPlus.Language, ObjectiveC.Language }
            };

            return Enumerable.Empty<PatternRoot>();
        }
    }
}
