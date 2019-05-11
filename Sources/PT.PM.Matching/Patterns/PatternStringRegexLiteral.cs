using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst, IRegexPattern, ITerminalPattern
    {
        [JsonIgnore]
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

        [JsonIgnore]
        public override bool Any => Regex.ToString() == Default;

        public override string ToString() => $@"<""{(Any ? "" : Regex.ToString())}"">";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (ust is StringLiteral stringLiteral)
            {
                List<TextSpan> matches = stringLiteral.Text is null
                    ? Regex.MatchRegex(stringLiteral.CurrentSourceFile, stringLiteral.TextSpan,
                            stringLiteral.EscapeCharsLength)
                    : Regex.MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength);

                return matches.Count > 0 ? context.AddMatches(matches) : context.Fail();
            }

            if (context.UstConstantFolder != null &&
                context.UstConstantFolder.TryGetOrFold(ust, out FoldResult foldResult))
            {
                context.MatchedWithFolded = true;
                if (foldResult.Value is string stringValue)
                {
                    List<TextSpan> matches = Regex.MatchRegex(stringValue, escapeCharsLength: 1);

                    matches = UstUtils.GetAlignedTextSpan(1, foldResult.TextSpans, matches, ust.TextSpan.Start);

                    return matches.Count > 0
                        ? context.AddMatches(matches)
                        : context.Fail();
                }
            }

            return context.Fail();
        }
    }
}
