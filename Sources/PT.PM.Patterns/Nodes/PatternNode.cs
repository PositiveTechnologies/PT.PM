using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Common.Nodes;

namespace PT.PM.Patterns.Nodes
{
    public class PatternNode : UstNode
    {
        public override NodeType NodeType => NodeType.PatternNode;

        public IList<PatternVarDef> Vars { get; set; } = new List<PatternVarDef>();

        public UstNode Node { get; set; }

        public PatternNode(UstNode data, IList<PatternVarDef> vars)
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
