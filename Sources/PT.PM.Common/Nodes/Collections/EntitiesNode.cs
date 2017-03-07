﻿using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Collections
{
    public class EntitiesNode : CollectionNode<EntityDeclaration>
    {
        public override NodeType NodeType => NodeType.EntitiesNode;

        public EntitiesNode(IList<EntityDeclaration> entities, TextSpan textSpan, FileNode fileNode)
            : base(entities, textSpan, fileNode)
        {

        }

        public EntitiesNode(IList<EntityDeclaration> entities)
            : base(entities)
        {
        }

        public EntitiesNode()
            : base()
        {
        }
    }
}
