using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst<StringLiteral>, IRegexPattern
    {
        public string Default => @".*";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public PatternStringRegexLiteral()
            : this("")
        {
        }

        public PatternStringRegexLiteral(string regexString, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            RegexString = regexString;
        }

        public override bool Any => Regex.ToString() == Default;

        public override string ToString() => $@"<""{(Any ? "" : Regex.ToString())}"">";

        public override MatchContext Match(StringLiteral stringLiteral, MatchContext context)
        {
            IEnumerable<TextSpan> matches = Regex
                .MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength);

            if (stringLiteral.InitialTextSpans?.Any() ?? false)
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
                    TextSpan textSpan = TextSpan.Zero;

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

                        if (!textSpan.IsZero && location.End <= rightBound)
                        {
                            result.Add(new TextSpan(textSpan.Start, location.Length + offset, textSpan.CodeFile));
                            break;
                        }
                    }

                    if (textSpan.IsZero)
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
