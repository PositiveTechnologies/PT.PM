using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdRegexToken : PatternUst, IRegexPattern, ITerminalPattern
    {
        private Regex regex;
        private Regex caseInsensitiveRegex;

        [JsonIgnore]
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

        public PatternIdRegexToken(string regexId, TextSpan textSpan = default)
            : base(textSpan)
        {
            RegexString = regexId;
        }

        [JsonIgnore]
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

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            if (!(ust is IdToken || ust is TypeToken))
            {
                return context.Fail();
            }

            Regex localRegex = ust.Root.Language.IsCaseInsensitive()
                ? caseInsensitiveRegex
                : regex;
            string tokenText = ((Token)ust).TextValue;

            return localRegex.Match(tokenText).Success
                ? context.AddMatch(ust)
                : context.Fail();
        }
    }
}
