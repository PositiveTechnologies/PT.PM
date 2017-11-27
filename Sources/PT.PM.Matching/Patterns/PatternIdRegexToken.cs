using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdRegexToken : PatternUst<Token>
    {
        private Regex regex;
        private Regex caseInsensitiveRegex;

        public Regex IdRegex
        {
            get
            {
                return regex;
            }
            set
            {
                regex = value;
                caseInsensitiveRegex = new Regex(value.ToString(),
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdRegexToken()
            : this(@"\w+")
        {
        }

        public PatternIdRegexToken(string regexId, TextSpan textSpan = default(TextSpan))
            : this(new Regex(string.IsNullOrEmpty(regexId) ? @"\w+" : regexId, RegexOptions.Compiled), textSpan)
        {
        }

        public PatternIdRegexToken(Regex regex, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            IdRegex = regex;
        }

        public bool Any => regex.ToString() == @"\w+";

        public override string ToString()
        {
            var regexString = regex.ToString();
            if (regex.Options.HasFlag(RegexOptions.IgnoreCase) && !regexString.StartsWith("(?i)"))
            {
                regexString += "(?i)";
            }
            return $"<[{regexString}]>";
        }

        public override MatchingContext Match(Token token, MatchingContext context)
        {
            MatchingContext newContext;

            Regex regex = token.Root.Language.IsCaseInsensitive
                ? caseInsensitiveRegex
                : this.regex;
            string tokenText = token.TextValue;
            TextSpan textSpan = regex.Match(tokenText).GetTextSpan(tokenText);
            if (!textSpan.IsEmpty)
            {
                newContext = context.AddMatch(token);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
