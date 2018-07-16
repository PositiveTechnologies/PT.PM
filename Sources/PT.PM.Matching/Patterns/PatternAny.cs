using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternAny : PatternUst<Ust>, IRegexPattern
    {
        public static PatternAny Instance = new PatternAny();

        public string Default => ".*";

        public string RegexString
        {
            get => Regex.ToString();
            set => Regex = new Regex(string.IsNullOrEmpty(value) ? Default : value, RegexOptions.Compiled);
        }

        public Regex Regex { get; private set; }

        public override bool Any => Regex == null || RegexString.Equals(Default);

        public PatternAny()
        {
        }

        public PatternAny(string regex)
        {
            RegexString = regex;
        }

        public override MatchContext Match(Ust ust, MatchContext context)
        {
            if (Any)
            {
                if (ust == null)
                {
                    return context.MakeSuccess();
                }
                return context.AddMatch(ust);
            }

            if (ust.Children.Length > 0)
            {
                return context.Fail();
            }

            string ustString = ust.ToString();
            var matches = Regex.MatchRegex(ustString, (ust as StringLiteral)?.EscapeCharsLength ?? 0);

            if (ust.InitialTextSpans?.Any() ?? false)
            {
                List<TextSpan> result = new List<TextSpan>();
                var initialTextSpans = ust.InitialTextSpans.OrderBy(x => x).ToList();
                var escapeLength = (ust as StringLiteral)?.EscapeCharsLength ?? 1;

                foreach (TextSpan location in matches)
                {
                    int offset = 0;
                    int leftBound = 1;
                    int rightBound =
                        initialTextSpans[0].Length - 2 * escapeLength + 1;
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
                matches = result.ToArray();
            }

            matches = matches.Select(location => location.AddOffset(ust.TextSpan.Start)).ToArray();

            return matches.Length > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }

        public override string ToString()
        {
            return $@"<#""{(Any ? "" : RegexString + "#")}"">";
        }
    }
}
