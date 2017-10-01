using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternBooleanLiteral : PatternBase
    {
        public bool Value { get; set; }

        public bool Any { get; set; } = true;

        public PatternBooleanLiteral()
        {
        }

        public PatternBooleanLiteral(bool? value = null, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Any = value == null;
            Value = value.Value;
        }

        public override string ToString()
        {
            if (Any)
            {
                return "True <|> False";
            }

            return Value.ToString();
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is BooleanLiteral booleanLiteral &&
                (Any || Value.Equals(booleanLiteral.Value)))
            {
                newContext = context.AddMatch(ust);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
