using MessagePack;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class BinaryOperatorExpression : Expression
    {
        [Key(UstFieldOffset)]
        public Expression Left { get; set; }

        [Key(UstFieldOffset + 1)]
        public BinaryOperatorLiteral Operator { get; set; }

        [Key(UstFieldOffset + 2)]
        public Expression Right { get; set; }

        public BinaryOperatorExpression()
        {
        }

        public BinaryOperatorExpression(Expression left, BinaryOperatorLiteral op, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override Ust[] GetChildren() => new Ust[] { Left, Operator, Right };

        public override Expression[] GetArgs() => new [] { Left, Operator, Right };

        public override string ToString() => $"{Left} {Operator} {Right}";
    }
}
