using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Collections
{
    [MessagePackObject]
    public class Collection : CollectionNode<Ust>
    {
        public Collection()
            : base()
        {
        }

        public Collection(IEnumerable<Ust> collection)
            : base(collection)
        {
        }
    }
}
