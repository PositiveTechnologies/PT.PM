using System.Collections.Generic;

namespace PT.PM.Common.Nodes
{
    public interface IAttributable
    {
        List<Attribute> Attributes { get; set; }
    }
}
