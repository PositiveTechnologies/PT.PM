using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst, IRegexPattern, ITerminalPattern
    {
        public string Default => ".*";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public PatternStringRegexLiteral()
            : this("")
        {
        }

        public PatternStringRegexLiteral(string regexString, TextSpan textSpan = default)
            : base(textSpan)
        {
            RegexString = regexString;
        }

        public override bool Any => Regex.ToString() == Default;

        public override string ToString() => $@"<""{(Any ? "" : Regex.ToString())}"">";

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is StringLiteral stringLiteral)
            {
                return MatchContext(context, stringLiteral.Text, stringLiteral.EscapeCharsLength, null, stringLiteral.TextSpan.Start);
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryFold(ust, out FoldResult foldResult))
            {
                context.MatchedWithFolded = true;
                if (foldResult.Value is string stringValue)
                {
                    return MatchContext(context, stringValue, 1, foldResult.TextSpans, ust.TextSpan.Start);
                }
            }

            return context.Fail();
        }

        private MatchContext MatchContext(MatchContext context, string text, int escapeCharsLength,
            List<TextSpan> foldedTextSpans, int startOffset)
        {
            List<TextSpan> matches = Regex.MatchRegex(text, escapeCharsLength);

            matches = UstUtils.GetAlignedTextSpan(escapeCharsLength, foldedTextSpans, matches, startOffset);

            return matches.Count > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
