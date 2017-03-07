using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternMultipleExpressions : Expression
    {
        public override NodeType NodeType => NodeType.PatternMultipleExpressions;

        public PatternMultipleExpressions()
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

            if (other is Expression)
            {
                return 0;
            }
            else
            {
                return NodeType - other.NodeType;
            }
        }

        public override string ToString() => "#*";
    }
}
