﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Dsl
{
    public class DslNode : CollectionNode<UstNode>
    {
        public override NodeType NodeType => NodeType.DslNode;

        public Language? Language { get; set; }

        public PatternVarDef[] PatternVarDefs { get; set; }

        public DslNode(IList<UstNode> collection, TextSpan textSpan, FileNode fileNode)
            : base(collection, textSpan, fileNode)
        {
        }

        public DslNode(IList<UstNode> collection)
            : base(collection)
        {
        }

        public DslNode()
            : base()
        {
        }
    }
}
