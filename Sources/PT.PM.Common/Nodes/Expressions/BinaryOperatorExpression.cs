using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    public class BinaryOperatorExpression : Expression
    {
        public override NodeType NodeType => NodeType.BinaryOperatorExpression;

        public Expression Left { get; set; }

        public BinaryOperatorLiteral Operator { get; set; }

        public Expression Right { get; set; }

        public BinaryOperatorExpression(Expression left, BinaryOperatorLiteral op, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public BinaryOperatorExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode> { Left, Operator, Right };
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }
    }
}
