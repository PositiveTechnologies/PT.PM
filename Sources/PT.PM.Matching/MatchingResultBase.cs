using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using System.Collections.Generic;

namespace PT.PM.Matching
{
    public abstract class MatchingResultBase<P>
        where P : PatternBase
    {
        public P Pattern { get; set; }

        public List<UstNode> Nodes { get; set; }
    }
}
