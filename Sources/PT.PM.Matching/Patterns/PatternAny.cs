using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternUst<Ust>, IRegexPattern
    {
        public static PatternAny Instance = new PatternAny();

        public string Default => ".*";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public override bool Any => Regex == null || RegexString.Equals(Default);

        public PatternAny()
        {
        }

        public PatternAny(string regex)
        {
            RegexString = regex;
        }

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (Any)
            {
                if (ust == null)
                {
                    return context.MakeSuccess();
                }
                return context.AddMatch(ust);
            }

            string ustString = ust.ToString();
            var matches = Regex.MatchRegex(ustString, (ust as StringLiteral)?.EscapeCharsLength ?? 0);

            if (ust.InitialTextSpans?.Any() ?? false)
            {
                matches = UstUtils.GetAlignedTextSpan(ust, matches).ToArray();
            }

            matches = matches.Select(location => location.AddOffset(ust.TextSpan.Start)).ToArray();

            return matches.Length > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }

        public override string ToString()
        {
            return $@"<""#{(Any ? "" : RegexString + "#")}"">";
        }
    }
}
