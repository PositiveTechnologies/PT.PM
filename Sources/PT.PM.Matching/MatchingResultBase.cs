using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public abstract class MatchingResultBase<TPattern>
        where TPattern : PatternBase
    {
        public TPattern Pattern { get; set; }

        public List<UstNode> Nodes { get; set; }
    }
}
