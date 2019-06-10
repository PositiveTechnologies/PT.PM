using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Collections
{
    [MessagePackObject]
    public class Collection : CollectionNode<Ust>
    {
        [Key(0)] public override UstType UstType => UstType.Collection;

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
