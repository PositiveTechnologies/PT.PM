using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternBase
    {
        private Regex regex;

        public string Regex
        {
            get => regex.ToString();
            set => regex = new Regex(value, RegexOptions.Compiled);
        }

        public PatternStringRegexLiteral()
            : this("")
        {
        }

        public PatternStringRegexLiteral(string regexString, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Regex = string.IsNullOrEmpty(regexString) ? @".*" : regexString;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => regex.ToString();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;
            if (ust is StringLiteral stringLiteral)
            {
                newContext = context.AddMatches(regex
                    .MatchRegex(stringLiteral.Text, isQuoted: true)
                    .Select(location => location.AddOffset(ust.TextSpan.Start)));
            }
            else
            {
                newContext = context.Fail();
            }
            return newContext;
        }
    }
}
