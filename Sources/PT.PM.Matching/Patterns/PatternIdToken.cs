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

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is Token token)
            {
                string tokenText = token.TextValue;
                if (ust.Root.Language.IsCaseInsensitive())
                {
                    TextSpan[] matchedLocations = caseInsensitiveRegex.MatchRegex(tokenText, true);
                    if (matchedLocations.Length > 0)
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
