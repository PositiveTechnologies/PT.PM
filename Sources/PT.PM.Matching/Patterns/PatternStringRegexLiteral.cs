using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternBase
    {
        public Regex StringRegex { get; set; }

        public PatternStringRegexLiteral()
            : this("")
        {
        }

        public PatternStringRegexLiteral(string regexString, TextSpan textSpan = default(TextSpan))
            : this(new Regex(string.IsNullOrEmpty(regexString) ? ".*" : regexString, RegexOptions.Compiled), textSpan)
        {
        }

        public PatternStringRegexLiteral(Regex regex, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            StringRegex = regex;
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => StringRegex.ToString();

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;
            if (ust is StringLiteral stringLiteral)
            {
                newContext = context.AddMatches(StringRegex
                    .MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength)
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
