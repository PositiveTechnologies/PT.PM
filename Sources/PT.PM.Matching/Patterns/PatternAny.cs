using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternUst<Ust>, IRegexPattern
    {
        public string Default => ".*";

        public string RegexString
        {
            get => Regex?.ToString() ?? "";
            set => Regex = value == null
                ? null
                : new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public override bool Any => Regex == null || RegexString.Equals(Default);

        public bool UseUstString { get; set; }

        public PatternAny()
        {
        }

        public PatternAny(string regex, TextSpan textSpan = default)
            : base(textSpan)
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

            string treeString;

            if (UseUstString)
            {
                treeString = ust.ToString();
            }
            else
            {
                treeString = ust.CurrentCodeFile?.GetSubstring(ust.TextSpan) ?? "";
            }

            var matches = Regex.MatchRegex(treeString, (ust as StringLiteral)?.EscapeCharsLength ?? 0);

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
            return Any ? "<#>" : $@"<#{RegexString}#>";
        }
    }
}
