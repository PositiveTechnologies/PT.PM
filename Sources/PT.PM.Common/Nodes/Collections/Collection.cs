using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
    public class Collection : CollectionNode<UstNode>
    {
        public override NodeType NodeType => NodeType.Collection;

        public Collection()
            : base()
        {
        }

        public Collection(IEnumerable<UstNode> collection)
            : base(collection)
        {
        }
    }
}
