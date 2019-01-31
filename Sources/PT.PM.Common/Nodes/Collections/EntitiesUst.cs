using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Collections
{
    [MessagePackObject]
    public class EntitiesUst : CollectionNode<EntityDeclaration>
    {
        public EntitiesUst(List<EntityDeclaration> entities, TextSpan textSpan)
            : base(entities, textSpan)
        {

        }

        public EntitiesUst(List<EntityDeclaration> entities)
            : base(entities)
        {
        }

        public EntitiesUst()
            : base()
        {
        }
    }
}
