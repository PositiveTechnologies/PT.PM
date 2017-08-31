using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Collections
{
    public abstract class CollectionNode<TUstNode> : UstNode
        where TUstNode : UstNode
    {
        public List<TUstNode> Collection { get; set; }

        protected CollectionNode(IEnumerable<TUstNode> collection, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
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

        public override UstNode[] GetChildren()
        {
            return Collection == null ? ArrayUtils<UstNode>.EmptyArray : Collection.ToArray();
        }
    }
}
