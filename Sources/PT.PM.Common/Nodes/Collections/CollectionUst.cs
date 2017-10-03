using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Collections
{
    public abstract class CollectionNode<TUstNode> : Ust
        where TUstNode : Ust
    {
        public List<TUstNode> Collection { get; set; }

        protected CollectionNode(IEnumerable<TUstNode> collection, TextSpan textSpan)
            : base(textSpan)
        {
            Collection = collection as List<TUstNode> ?? collection.ToList();
        }

        protected CollectionNode(IEnumerable<TUstNode> collection)
            : base()
        {
            Collection = collection as List<TUstNode> ?? collection.ToList();
        }

        protected CollectionNode()
            : base()
        {
            Collection = new List<TUstNode>();
        }

        public override Ust[] GetChildren()
        {
            return Collection == null ? ArrayUtils<Ust>.EmptyArray : Collection.ToArray();
        }
    }
}
