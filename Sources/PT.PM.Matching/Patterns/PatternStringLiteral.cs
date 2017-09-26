using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringLiteral : PatternBase
    {
        public string String { get; set; } = "";

        public PatternStringLiteral()
        {
        }

        public PatternStringLiteral(string @string, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            String = @string;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => String;

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;
            if (ust is StringLiteral stringLiteral && String.Equals(stringLiteral.Text))
            {
                match = context.AddLocation(ust.TextSpan);
            }
            else
            {
                match = context.Fail();
            }
            return match;
        }
    }
}
