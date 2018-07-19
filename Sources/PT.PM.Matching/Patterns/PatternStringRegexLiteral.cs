using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst<StringLiteral>, IRegexPattern
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

        public override MatchContext Match(StringLiteral stringLiteral, MatchContext context)
        {
            IEnumerable<TextSpan> matches = Regex
                .MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength);

            if (stringLiteral.InitialTextSpans?.Any() ?? false)
            {
                matches = UstUtils.GetAlignedTextSpan(stringLiteral, matches.ToArray());
            }

            matches = matches.Select(location => location.AddOffset(stringLiteral.TextSpan.Start));

            return matches.Count() > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
