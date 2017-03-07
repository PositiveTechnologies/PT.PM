using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Collections
{
    public abstract class CollectionNode<TAstNode> : UstNode
        where TAstNode : UstNode
    {
        public IList<TAstNode> Collection { get; set; }

        protected CollectionNode(IList<TAstNode> collection, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Collection = collection;
        }

        protected CollectionNode(IList<TAstNode> collection)
            : base()
        {
            Collection = collection;
        }

        protected CollectionNode()
            : base()
        {
            Collection = new List<TAstNode>();
        }

        public override UstNode[] GetChildren()
        {
            return Collection == null ? ArrayUtils<UstNode>.EmptyArray : Collection.ToArray();
        }
    }
}
