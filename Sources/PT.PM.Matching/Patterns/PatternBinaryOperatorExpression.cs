using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorExpression : PatternUst<BinaryOperatorExpression>
    {
        public PatternUst Left { get; set; }

        public PatternBinaryOperatorLiteral Operator { get; set; }

        public PatternUst Right { get; set; }

        public PatternBinaryOperatorExpression()
        {
        }

        public PatternBinaryOperatorExpression(PatternUst left, PatternBinaryOperatorLiteral op, PatternUst right,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override string ToString() => $"{Left} {Operator} {Right}";

        public override MatchingContext Match(BinaryOperatorExpression binaryOperatorExpression, MatchingContext context)
        {
            MatchingContext newContext = Left.MatchUst(binaryOperatorExpression.Left, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Operator.Match(binaryOperatorExpression.Operator, newContext);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Right.MatchUst(binaryOperatorExpression.Right, newContext);

            return newContext.AddUstIfSuccess(binaryOperatorExpression);
        }
    }
}
