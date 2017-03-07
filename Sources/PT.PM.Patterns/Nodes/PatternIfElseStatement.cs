using PT.PM.Common.Nodes.Statements;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternIfElseStatement : IfElseStatement
    {
        public override NodeType NodeType => NodeType.PatternIfElseStatement;

        public PatternIfElseStatement()
        {
        }

        public PatternIfElseStatement(Expression condition, BlockStatement trueStatement, TextSpan textSpan, FileNode fileNode)
            : base(condition, trueStatement, textSpan, fileNode)
        {
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternIfElseStatement)
            {
                return base.CompareTo(other);
            }

            // TODO: support for conditional operator
            if (other.NodeType != NodeType.IfElseStatement)
            {
                return NodeType - other.NodeType;
            }

            IfElseStatement otherIfElseStatement = (IfElseStatement)other;
            int conditionCompareResult = Condition.CompareTo(otherIfElseStatement.Condition);
            if (conditionCompareResult != 0)
            {
                return conditionCompareResult;
            }

            int trueStatementCompareResult = TrueStatement.CompareTo(otherIfElseStatement.TrueStatement);
            if (trueStatementCompareResult != 0)
            {
                return trueStatementCompareResult;
            }

            if (FalseStatement != null)
            { 
                return FalseStatement.CompareTo(otherIfElseStatement.FalseStatement);
            }

            return 0;
        }

        public override string ToString()
        {
            return $"if ({Condition}) {{ {TrueStatement} }}";
        }
    }
}
