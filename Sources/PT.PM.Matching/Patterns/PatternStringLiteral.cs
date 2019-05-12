using System.Collections.Generic;
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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is StringLiteral stringLiteral)
            {
                if (stringLiteral.Text is null)
                {
                    return string.CompareOrdinal(String, 0, ust.CurrentSourceFile.Data, stringLiteral.TextSpan.Start,
                               String.Length) == 0
                        ? context.AddMatch(stringLiteral.ViewTextSpan)
                        : context.Fail();
                }

                return String.Equals(stringLiteral.Text)
                    ? context.AddMatch(stringLiteral.ViewTextSpan)
                    : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryGetOrFold(ust, out FoldResult foldResult))
            {
                context.MatchedWithFolded = true;
                if (foldResult.Value is string stringValue)
                {
                    if (String.Equals(stringValue))
                    {
                        var matches = new List<TextSpan> {new TextSpan(0, String.Length)};
                        matches = MatchUtils.AlignTextSpans(foldResult.TextSpans, matches, 1);
                        context.AddMatches(matches);
                    }
                }
            }

            return context.Fail();
        }
    }
}
