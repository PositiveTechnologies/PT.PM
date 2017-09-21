using PT.PM.Common.Nodes;
using PT.PM.Matching.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public abstract class MatchingResultBase<TPattern>
    {
        public TPattern Pattern { get; set; }

        public List<Ust> Nodes { get; set; }
    }
}
