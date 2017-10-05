using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdToken : PatternUst
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
            : this("")
        {
        }

        public PatternIdToken(string id, TextSpan textSpan = default(TextSpan))
        {
            Id = id;
            TextSpan = textSpan;
        }

        public override string ToString() => Id;

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is Token token)
            {
                string tokenText = token.TextValue;
                if (ust.Root.Language.IsCaseInsensitive)
                {
                    TextSpan textSpan = caseInsensitiveRegex.Match(tokenText).GetTextSpan(tokenText);
                    if (!textSpan.IsEmpty)
                    {
                        newContext = context.AddMatch(ust);
                    }
                    else
                    {
                        newContext = context.Fail();
                    }
                }
                else if (id.Equals(tokenText))
                {
                    newContext = context.AddMatch(ust);
                }
                else
                {
                    newContext = context.Fail();
                }
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}
