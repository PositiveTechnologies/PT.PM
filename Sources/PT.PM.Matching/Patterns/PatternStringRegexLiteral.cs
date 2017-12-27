using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst<StringLiteral>
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

        public override string ToString() => $@"<""{StringRegex}"">";

        public override MatchContext Match(StringLiteral stringLiteral, MatchContext context)
        {
            IEnumerable<TextSpan> matches = StringRegex
                .MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength);

            if (stringLiteral.InitialTextSpans.Any())
            {
                List<TextSpan> result = new List<TextSpan>();
                stringLiteral.InitialTextSpans.Sort();

                foreach (var location in matches)
                {
                    int offset = 0;
                    int leftBound = 1;
                    int rightBound = stringLiteral.InitialTextSpans[0].Length - 1; // - 2 because of quotes and then + 1
                    TextSpan textSpan = TextSpan.Empty;

                    // Check first initial textspan separately
                    if (location.Start < rightBound && location.End > rightBound)
                    {
                        textSpan = location;
                    }

                    for (int i = 1; i < stringLiteral.InitialTextSpans.Count; i++)
                    {
                        var initTextSpan = stringLiteral.InitialTextSpans[i];
                        var prevTextSpan = stringLiteral.InitialTextSpans[i - 1];
                        leftBound += prevTextSpan.Length - 2;
                        rightBound += initTextSpan.Length - 2;
                        offset += initTextSpan.Start - prevTextSpan.End + 2;

                        if (location.Start < leftBound && location.End < leftBound)
                        {
                            break;
                        }

                        if (location.Start >= leftBound && location.Start < rightBound)
                        {
                            textSpan = location.AddOffset(offset);
                            if (location.End <= rightBound)
                            {
                                result.Add(textSpan);
                                break;
                            }
                        }

                        if (!textSpan.IsEmpty && location.End <= rightBound)
                        {
                            result.Add(new TextSpan(textSpan.Start, location.Length + offset));
                            break;
                        }
                    }

                    if (textSpan.IsEmpty)
                    {
                        result.Add(location);
                    }
                }
                matches = result;
            }

            matches = matches.Select(location => location.AddOffset(stringLiteral.TextSpan.Start));

            return matches.Count() > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}
