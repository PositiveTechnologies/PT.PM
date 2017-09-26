using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
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
