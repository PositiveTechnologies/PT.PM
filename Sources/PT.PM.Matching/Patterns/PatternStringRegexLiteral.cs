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
                var initialTextSpans = stringLiteral.InitialTextSpans.OrderBy(el => el).ToList();
                var escapeLength = stringLiteral.EscapeCharsLength;

                foreach (TextSpan location in matches)
                {
                    int offset = 0;
                    int leftBound = 1;
                    int rightBound =
                        initialTextSpans[0].Length - 2 * escapeLength + 1; // - quotes length and then + 1
                    TextSpan textSpan = TextSpan.Empty;

                    // Check first initial textspan separately
                    if (location.Start < rightBound && location.End > rightBound)
                    {
                        textSpan = location;
                    }

                    for (int i = 1; i < initialTextSpans.Count; i++)
                    {
                        var initTextSpan = initialTextSpans[i];
                        var prevTextSpan = initialTextSpans[i - 1];
                        leftBound += prevTextSpan.Length - 2 * escapeLength;
                        rightBound += initTextSpan.Length - 2 * escapeLength;
                        offset += initTextSpan.Start - prevTextSpan.End + 2 * escapeLength;

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
