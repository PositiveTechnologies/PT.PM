using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public interface ISemanticsCollector
    {
        SemanticsInfo Collect(IEnumerable<ParseTree> asts);
    }
}
