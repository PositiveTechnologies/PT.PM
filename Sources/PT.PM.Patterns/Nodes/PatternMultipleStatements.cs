using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Patterns.Nodes
{
    public class PatternMultipleStatements : Statement
    {
        public override NodeType NodeType => NodeType.PatternMultipleStatements;

        public PatternMultipleStatements()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)other.NodeType;
            }

            if (other is Statement)
            {
                return 0;
            }
            else
            {
                return NodeType - other.NodeType;
            }
        }

        public override string ToString() => "...;";
    }
}
