using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Matching.Patterns
{
    public interface IPatternUst
    {
        bool Match(Ust ustNode, MatchingState matchingState);
    }
}
