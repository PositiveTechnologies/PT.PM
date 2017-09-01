using PT.PM.Common.Nodes;
using PT.PM.Patterns.Nodes;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public abstract class MatchingResultBase<TPattern>
    {
        public TPattern Pattern { get; set; }

        public List<UstNode> Nodes { get; set; }
    }
}
