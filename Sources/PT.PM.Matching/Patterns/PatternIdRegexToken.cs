using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdRegexToken : PatternBase
    {
        private Regex regex;
        private Regex caseInsensitiveRegex;

        public string Id
        {
            get
            {
                return regex.ToString();
            }
            set
            {
                regex = new Regex(value, RegexOptions.Compiled);
                caseInsensitiveRegex = value.StartsWith("(?i)")
                    ? regex
                    : new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdRegexToken()
            : this(@"\w+")
        {
        }

        public PatternIdRegexToken(string regexId, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Id = string.IsNullOrEmpty(regexId) ? @"\w+" : regexId;
        }

        public override Ust[] GetChildren() => ArrayUtils<Expression>.EmptyArray;

        public override string ToString() => Id;

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext match;
            if (ust is Token token)
            {
                Regex regex = ust.Root.Language.IsCaseInsensitive()
                    ? caseInsensitiveRegex
                    : this.regex;
                TextSpan[] matchedLocations = regex.MatchRegex(token.TextValue, true);
                if (matchedLocations.Length > 0)
                {
                    match = context.AddUst(ust);
                }
                else
                {
                    match = context.Fail();
                }
            }
            else
            {
                match = context.Fail();
            }
            return match;
        }
    }
}
