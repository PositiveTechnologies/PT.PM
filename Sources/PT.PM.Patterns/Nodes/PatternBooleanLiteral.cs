using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Patterns.Nodes
{
    public class PatternBooleanLiteral : BooleanLiteral
    {
        public override NodeType NodeType => NodeType.PatternBooleanLiteral;

        public bool Any { get; set; }

        public PatternBooleanLiteral(bool? value = null)
        {
            Any = value == null;
            Value = value.Value;
        }

        public PatternBooleanLiteral()
        {
            Any = true;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternBooleanLiteral)
            {
                return Any == ((PatternBooleanLiteral)other).Any ? 0 : Value.CompareTo(((BooleanLiteral)other).Value);
            }

            if (other.NodeType != NodeType.BooleanLiteral)
            {
                return NodeType - other.NodeType;
            }

            var result = Any ? 0 : Value.CompareTo(((BooleanLiteral)other).Value);
            return result;
        }
    }
}
