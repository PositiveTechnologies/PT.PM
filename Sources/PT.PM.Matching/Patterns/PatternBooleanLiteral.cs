using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternBooleanLiteral : PatternUst, ITerminalPattern
    {
        public bool? Boolean { get; set; }

        public PatternBooleanLiteral()
        {
        }

        public PatternBooleanLiteral(bool? value = null, TextSpan textSpan = default)
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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            var booleanLiteral = ust as BooleanLiteral;
            if (booleanLiteral == null)
            {
                return context.Fail();
            }
            
            return (Boolean == null || Boolean.Value.Equals(booleanLiteral.Value))
                ? context.AddMatch(booleanLiteral)
                : context.Fail();
        }
    }
}
