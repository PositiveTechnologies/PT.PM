using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
    public class EntitiesNode : CollectionNode<EntityDeclaration>
    {
        public override NodeType NodeType => NodeType.EntitiesNode;

        public EntitiesNode(List<EntityDeclaration> entities, TextSpan textSpan)
            : base(entities, textSpan)
        {

        }

        public EntitiesNode(List<EntityDeclaration> entities)
            : base(entities)
        {
        }

        public EntitiesNode()
            : base()
        {
        }
    }
}
