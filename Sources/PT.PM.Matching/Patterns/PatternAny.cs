using System.Collections.Generic;
using System.Text.RegularExpressions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternUst, IRegexPattern, ITerminalPattern
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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (Any)
            {
                return ust == null ? context.MakeSuccess() : context.AddMatch(ust);
            }

            string treeString = UseUstString
                ? ust.ToString()
                : ust.CurrentCodeFile?.GetSubstring(ust.TextSpan) ?? "";

            int escapeCharsLength = (ust as StringLiteral)?.EscapeCharsLength ?? 0;
            List<TextSpan> matches = Regex.MatchRegex(treeString, escapeCharsLength);
            matches = UstUtils.GetAlignedTextSpan(escapeCharsLength, ust.InitialTextSpans, matches, ust.TextSpan.Start);

            return matches.Count > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }

        public override string ToString()
        {
            return Any ? "<#>" : $@"<#{RegexString}#>";
        }
    }
}
