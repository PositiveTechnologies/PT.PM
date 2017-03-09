using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Collections
{
    public abstract class CollectionNode<TUstNode> : UstNode
        where TUstNode : UstNode
    {
        public IList<TUstNode> Collection { get; set; }

        protected CollectionNode(IList<TUstNode> collection, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Collection = collection;
        }

        protected CollectionNode(IList<TUstNode> collection)
            : base()
        {
            Collection = collection;
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
