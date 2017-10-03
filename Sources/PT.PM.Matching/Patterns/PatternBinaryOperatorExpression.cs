using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorExpression : PatternUst
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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is BinaryOperatorExpression binaryOperatorExpression)
            {
                newContext = Left.Match(binaryOperatorExpression.Left, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Operator.Match(binaryOperatorExpression.Operator, newContext);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Right.Match(binaryOperatorExpression.Right, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}
