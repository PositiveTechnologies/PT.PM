using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternUnaryOperatorLiteral : PatternUst<UnaryOperatorLiteral>
    {
        public UnaryOperator UnaryOperator { get; set; }

        public bool IsPostfix { get; set; }

        public PatternUnaryOperatorLiteral()
            : this(UnaryOperator.None, false)
        {
        }

        public PatternUnaryOperatorLiteral(string opText, bool isPostfix, TextSpan textSpan = default)
            : base(textSpan)
        {
            UnaryOperator op;
            if (isPostfix)
            {
                UnaryOperatorLiteral.PostfixTextUnaryOperator.TryGetValue(opText, out op);
            }
            else
            {
                UnaryOperatorLiteral.PrefixTextUnaryOperator.TryGetValue(opText, out op);
            }
            IsPostfix = isPostfix;
            UnaryOperator = op;
        }

        public PatternUnaryOperatorLiteral(UnaryOperator op, bool isPostfix, TextSpan textSpan = default)
            : base(textSpan)
        {
            UnaryOperator = op;
            IsPostfix = isPostfix;
        }

        public override string ToString()
        {
            var result = "";

            result = UnaryOperatorLiteral.PrefixTextUnaryOperator.FirstOrDefault(pair => pair.Value == UnaryOperator).Key;

            if (string.IsNullOrEmpty(result))
            {
                result = UnaryOperatorLiteral.PostfixTextUnaryOperator.FirstOrDefault(pair => pair.Value == UnaryOperator).Key;
            }

            return string.IsNullOrEmpty(result) ? "None" : result;
        }

        public override MatchContext Match(UnaryOperatorLiteral UnaryOperatorLiteral, MatchContext context)
        {
            return UnaryOperator.Equals(UnaryOperatorLiteral.UnaryOperator)
                ? context.AddMatch(UnaryOperatorLiteral)
                : context.Fail();
        }
    }
}
