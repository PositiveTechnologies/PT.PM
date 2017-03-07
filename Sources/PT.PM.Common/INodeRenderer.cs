using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public interface INodeRenderer<T>
        where T : Node
    {
        string Render(T node);
    }
}
