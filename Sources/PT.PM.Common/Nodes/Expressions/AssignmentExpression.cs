using System;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class AssignmentExpression : Expression
    {
        public override NodeType NodeType => NodeType.AssignmentExpression;

        public Expression Left { get; set; }

        public Expression Right { get; set; }

        public AssignmentExpression(Expression left, Expression right, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Left = left;
            Right = right;
        }

        public AssignmentExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode> {Left, Right};
            return result.ToArray();
        }

        public override string ToString()
        {
            return Right == null ? Left.ToString() : $"{Left} = {Right}";
        }
    }
}
