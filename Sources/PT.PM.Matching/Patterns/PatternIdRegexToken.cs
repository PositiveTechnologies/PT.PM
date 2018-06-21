using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdRegexToken : PatternUst<Token>, IRegexPattern
    {
        private Regex regex;
        private Regex caseInsensitiveRegex;

        public string Default => @"\w+";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex
        {
            get => regex;
            set
            {
                regex = value;
                caseInsensitiveRegex = new Regex(value.ToString(),
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdRegexToken()
            : this("")
        {
        }

        public PatternIdRegexToken(string regexId, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            RegexString = regexId;
        }

        public override bool Any => regex.ToString() == Default;

        public override string ToString()
        {
            string regexString = Any ? "" : regex.ToString();
            if (regex.Options.HasFlag(RegexOptions.IgnoreCase) && !regexString.StartsWith("(?i)"))
            {
                regexString += "(?i)";
            }
            return $"<[{regexString}]>";
        }

        public override MatchContext Match(Token token, MatchContext context)
        {
            Regex regex = token.Root.Language.IsCaseInsensitive
                ? caseInsensitiveRegex
                : this.regex;
            string tokenText = token.TextValue;
            TextSpan textSpan = regex.Match(tokenText).GetTextSpan(tokenText);

            return !textSpan.IsZero
                ? context.AddMatch(token)
                : context.Fail();
        }
    }
}
