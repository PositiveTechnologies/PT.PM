using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Collections
{
    [MessagePackObject]
    public abstract class CollectionNode<TUst> : Ust
        where TUst : Ust
    {
        [Key(UstFieldOffset)]
        public List<TUst> Collection { get; set; }

        protected CollectionNode(IEnumerable<TUst> collection, TextSpan textSpan)
            : base(textSpan)
        {
            Collection = collection as List<TUst> ?? collection.ToList();
        }

        protected CollectionNode(IEnumerable<TUst> collection)
            : base()
        {
            Collection = collection as List<TUst> ?? collection.ToList();
        }

        protected CollectionNode()
            : base()
        {
            Collection = new List<TUst>();
        }

        public override Ust[] GetChildren()
        {
            return Collection == null ? ArrayUtils<Ust>.EmptyArray : Collection.ToArray();
        }
    }
}
