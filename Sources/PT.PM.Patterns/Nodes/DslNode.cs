using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using System.Collections.Generic;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Dsl
{
    public class DslNode : CollectionNode<UstNode>
    {
        public override NodeType NodeType => NodeType.DslNode;

        public Language? Language { get; set; }

        public List<PatternVarDef> PatternVarDefs { get; set; }

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
