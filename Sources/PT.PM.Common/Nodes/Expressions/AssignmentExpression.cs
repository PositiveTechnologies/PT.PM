using MessagePack;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class AssignmentExpression : Expression
    {
        [Key(UstFieldOffset)]
        public Expression Left { get; set; }

        [Key(UstFieldOffset + 1)]
        public BinaryOperatorLiteral Operator { get; set; }

        [Key(UstFieldOffset + 2)]
        public Expression Right { get; set; }

        public AssignmentExpression(Expression left, Expression right, TextSpan textSpan)
            : base(textSpan)
        {
            Left = left;
            Right = right;
        }

        public AssignmentExpression()
        {
        }

        public AssignmentExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public override Ust[] GetChildren()
        {
            if (Operator != null)
            {
                return new Ust[] { Left, Operator, Right};
            }
            return new Ust[] { Left, Right };
        }


        public override Expression[] GetArgs() => new Expression[] { Left, Right };

        public override string ToString()
        {
            return Right == null
                ? Left == null
                ? $" {Operator}= "
                : Left.ToString()
                : $"{Left} {Operator}= {Right}";
        }
    }
}
