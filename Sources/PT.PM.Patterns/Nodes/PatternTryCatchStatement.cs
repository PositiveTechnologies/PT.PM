using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;

namespace PT.PM.Patterns.Nodes
{
    public class PatternTryCatchStatement : Statement
    {
        public override NodeType NodeType => NodeType.PatternTryCatchStatement;

        public PatternTryCatchStatement()
        {
        }

        public PatternTryCatchStatement(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternTryCatchStatement)
            {
                return 0;
            }

            if (other.NodeType != NodeType.TryCatchStatement)
            {
                return NodeType - other.NodeType;
            }

            var otherTryCatch = (TryCatchStatement)other;
            if (otherTryCatch.CatchClauses == null)
            {
                return -1;
            }
            else
            {
                if (otherTryCatch.CatchClauses.Any(catchClause => catchClause.Body.Statements.Count() == 0))
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override string ToString()
        {
            return $"try catch {{ }}";
        }
    }
}
