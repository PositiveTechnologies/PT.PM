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

        public override MatchingContext Match(BooleanLiteral booleanLiteral, MatchingContext context)
        {
            MatchingContext newContext;

            if (Boolean == null || Boolean.Value.Equals(booleanLiteral.Value))
            {
                newContext = context.AddMatch(booleanLiteral);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
