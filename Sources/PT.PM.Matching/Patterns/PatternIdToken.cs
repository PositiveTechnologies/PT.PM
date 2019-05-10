using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdToken : PatternUst, ITerminalPattern
    {
        private string id = "";
        private Regex caseInsensitiveRegex;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                caseInsensitiveRegex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdToken()
            : this("")
        {
        }

        public PatternIdToken(string id, TextSpan textSpan = default)
        {
            Id = id;
            TextSpan = textSpan;
        }

        public override string ToString() => Id;

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var token = ust as Token;
            if (token == null)
            {
                return context.Fail();
            }

            string tokenText = token.TextValue;
            if (token.Root.Language.IsCaseInsensitive())
            {
                TextSpan textSpan = caseInsensitiveRegex.Match(tokenText).GetTextSpan();
                if (!textSpan.IsZero)
                {
                    return context.AddMatch(token);
                }

                return context.Fail();
            }

            if (id.Equals(tokenText))
            {
                return context.AddMatch(token);
            }

            return context.Fail();
        }
    }
}
