using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternBooleanLiteral : PatternUst<BooleanLiteral>
    {
        public bool? Boolean { get; set; }

        public PatternBooleanLiteral()
        {
        }

        public PatternBooleanLiteral(bool? value = null, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Boolean = value;
        }

        public override string ToString()
        {
            if (Boolean == null)
            {
                return "True <|> False";
            }

            return Boolean.ToString();
        }

        public override MatchContext Match(BooleanLiteral booleanLiteral, MatchContext context)
        {
            return (Boolean == null || Boolean.Value.Equals(booleanLiteral.Value))
                ? context.AddMatch(booleanLiteral)
                : context.Fail();
        }
    }
}
