using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorExpression : PatternBase
    {
        public PatternBase Left { get; set; }

        public BinaryOperatorLiteral Operator { get; set; }

        public PatternBase Right { get; set; }

        public PatternBinaryOperatorExpression()
        {
        }

        public PatternBinaryOperatorExpression(PatternBase left, BinaryOperatorLiteral op, PatternBase right,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override Ust[] GetChildren() => new Ust[] { Left, Operator, Right };

        public override string ToString() => $"{Left} {Operator} {Right}";

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.BinaryOperatorExpression)
            {
                return false;
            }

            var binaryOperatorExpression = (BinaryOperatorExpression)ust;
            return Left.Match(binaryOperatorExpression.Left, context) &&
                   Operator.Equals(binaryOperatorExpression.Operator) &&
                   Right.Match(binaryOperatorExpression.Right, context);
        }
    }
}
