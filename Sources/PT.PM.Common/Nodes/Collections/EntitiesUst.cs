using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
    public class EntitiesUst : CollectionNode<EntityDeclaration>
    {
        public override UstKind Kind => UstKind.EntitiesUst;

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
