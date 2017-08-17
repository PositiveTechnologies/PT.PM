using System;
using System.Linq;
using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;

namespace PT.PM.Patterns.Nodes
{
    public class PatternTryCatchStatement : Statement
    {
        public override NodeType NodeType => NodeType.PatternTryCatchStatement;

        public List<TypeToken> ExceptionTypes { get; set; }

        public bool IsCatchBodyEmpty { get; set; }

        public PatternTryCatchStatement()
        {
            ExceptionTypes = new List<TypeToken>();
            IsCatchBodyEmpty = true;
        }

        public PatternTryCatchStatement(List<TypeToken> exceptionTypes, bool isCatchBodyEmpty,
            TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            ExceptionTypes = exceptionTypes ?? throw new ArgumentNullException("exceptionTypes");
            IsCatchBodyEmpty = isCatchBodyEmpty;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternTryCatchStatement)
            {
                var otherPatternTryCatch = (PatternTryCatchStatement)other;
                int result = Convert.ToInt32(IsCatchBodyEmpty) - Convert.ToInt32(otherPatternTryCatch.IsCatchBodyEmpty);
                if (result != 0)
                {
                    return result;
                }

                return ExceptionTypes.CompareTo(otherPatternTryCatch.ExceptionTypes);
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
                bool result = otherTryCatch.CatchClauses.Any(catchClause =>
                {
                    if (IsCatchBodyEmpty && catchClause.Body.Statements.Any())
                    {
                        return false;
                    }

                    return !ExceptionTypes.Any() || ExceptionTypes.Any(type => type.CompareTo(catchClause.Type) == 0);
                });

                if (result)
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
