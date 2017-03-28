using System.Collections.Generic;
using PT.PM.Common.Nodes;

namespace PT.PM.Patterns.Nodes
{
    public class PatternNode : UstNode
    {
        public override NodeType NodeType => NodeType.PatternNode;

        public List<PatternVarDef> Vars { get; set; } = new List<PatternVarDef>();

        public UstNode Node { get; set; }

        public PatternNode(UstNode data, List<PatternVarDef> vars)
        {
            Node = data;
            Vars = vars;
        }

        public PatternNode()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Node);
            result.AddRange(Vars);
            return result.ToArray();
        }
    }
}
