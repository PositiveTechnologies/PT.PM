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
        public override UstKind Kind => UstKind.PatternBooleanLiteral;

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

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternBooleanLiteral)
            {
                return Any == ((PatternBooleanLiteral)other).Any ? 0 : Value.CompareTo(((BooleanLiteral)other).Value);
            }

            if (other.Kind != UstKind.BooleanLiteral)
            {
                return Kind - other.Kind;
            }

            var result = Any ? 0 : Value.CompareTo(((BooleanLiteral)other).Value);
            return result;
        }
    }
}
