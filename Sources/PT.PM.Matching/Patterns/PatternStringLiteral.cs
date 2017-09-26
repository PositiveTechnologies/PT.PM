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

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.StringLiteral)
            {
                return false;
            }

            var stringLiteral = (StringLiteral)ust;
            bool match = String.Equals(stringLiteral.Text);
            if (match)
            {
                context.AddLocation(ust.TextSpan);
            }
            return match;
        }
    }
}
