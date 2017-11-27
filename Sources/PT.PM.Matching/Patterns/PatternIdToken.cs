using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdToken : PatternUst<Token>
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

        public override MatchingContext Match(Token token, MatchingContext context)
        {
            MatchingContext newContext;

            if (!(token is CommentLiteral))
            {
                string tokenText = token.TextValue;
                if (token.Root.Language.IsCaseInsensitive)
                {
                    TextSpan textSpan = caseInsensitiveRegex.Match(tokenText).GetTextSpan(tokenText);
                    if (!textSpan.IsEmpty)
                    {
                        newContext = context.AddMatch(token);
                    }
                    else
                    {
                        newContext = context.Fail();
                    }
                }
                else if (id.Equals(tokenText))
                {
                    newContext = context.AddMatch(token);
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
