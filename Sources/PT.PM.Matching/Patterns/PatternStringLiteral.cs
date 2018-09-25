using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringLiteral : PatternUst<StringLiteral>, ITerminalPattern
    {
        public string String { get; set; } = "";

        public PatternStringLiteral()
        {
        }

        public PatternStringLiteral(string @string, TextSpan textSpan = default)
            : base(textSpan)
        {
            String = @string;
        }

        public override string ToString() => '"' + String + '"';

        public override MatchContext Match(StringLiteral stringLiteral, MatchContext context)
        {
            return String.Equals(stringLiteral.Text)
                ? context.AddMatch(stringLiteral)
                : context.Fail();
        }
    }
}
