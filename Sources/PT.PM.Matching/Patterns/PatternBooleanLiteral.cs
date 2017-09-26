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

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

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
            if (ust?.Kind != UstKind.BooleanLiteral)
            {
                return context.Fail();
            }

            bool otherValue = ((BooleanLiteral)ust).Value;
            if (Any || Value.Equals(otherValue))
            {
                return context.AddLocation(ust.TextSpan);
            }

            return context.Fail();
        }
    }
}
