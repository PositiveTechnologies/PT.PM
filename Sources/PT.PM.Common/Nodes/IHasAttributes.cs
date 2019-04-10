using System.Collections.Generic;

namespace PT.PM.Common.Nodes
{
    public interface IHasAttributes
    {
        List<Attribute> Attributes { get; set; }
    }
}
