using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringLiteral : PatternUst, ITerminalPattern
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

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is StringLiteral stringLiteral)
            {
                return String.Equals(stringLiteral.Text) ? context.AddMatch(stringLiteral) : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryGetOrFold(ust, out FoldResult foldingResult))
            {
                context.MatchedWithFolded = true;
                if (foldingResult.Value is string stringValue)
                {
                    return String.Equals(stringValue) ? context.AddMatches(foldingResult.TextSpans) : context.Fail();
                }
            }

            return context.Fail();
        }
    }
}
