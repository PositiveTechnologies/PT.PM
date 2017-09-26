using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdToken : PatternBase
    {
        private string id = "";
        private Regex caseInsensitiveRegex;

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                caseInsensitiveRegex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdToken()
        {
        }

        public PatternIdToken(string id, TextSpan textSpan = default(TextSpan))
        {
            Id = id;
            TextSpan = textSpan;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => Id;

        public override bool Match(Ust ust, MatchingContext context)
        {
            var token = ust as Token;
            if (token != null)
            {
                return false;
            }

            string tokenText = token.TextValue;
            if (ust.Root.Language.IsCaseInsensitive())
            {
                TextSpan[] matchedLocations = caseInsensitiveRegex.MatchRegex(tokenText, true);
                return matchedLocations.Length > 0;
            }
            else
            {
                return id.Equals(tokenText);
            }
        }
    }
}
