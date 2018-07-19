using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorLiteral : PatternUst<BinaryOperatorLiteral>
    {
        public BinaryOperator BinaryOperator { get; set; }

        public PatternBinaryOperatorLiteral()
            : this(BinaryOperator.Equal)
        {
        }

        public PatternBinaryOperatorLiteral(string opText, TextSpan textSpan = default)
            : base(textSpan)
        {
            BinaryOperator op;
            if (!BinaryOperatorLiteral.TextBinaryOperator.TryGetValue(opText, out op))
            {
                op = BinaryOperator.Equal;
            }
            BinaryOperator = op;
        }

        public PatternBinaryOperatorLiteral(BinaryOperator op, TextSpan textSpan = default)
            : base(textSpan)
        {
            BinaryOperator = op;
        }

        public override string ToString() =>
            BinaryOperatorLiteral.TextBinaryOperator.FirstOrDefault(pair => pair.Value == BinaryOperator).Key;

        public override MatchContext Match(BinaryOperatorLiteral binaryOperatorLiteral, MatchContext context)
        {
            return BinaryOperator.Equals(binaryOperatorLiteral.BinaryOperator)
                ? context.AddMatch(binaryOperatorLiteral)
                : context.Fail();
        }
    }
}
