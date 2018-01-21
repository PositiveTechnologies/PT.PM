using PT.PM.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching
{
    public static class PatternUtils
    {
        public static TextSpan[] MatchRegex(this Regex patternRegex, string text, int escapeCharsLength = 0)
        {
            MatchCollection matches = patternRegex.Matches(text);
            var result = new List<TextSpan>(matches.Count);
            foreach (Match match in matches)
            {
                TextSpan textSpan = match.GetTextSpan(text, escapeCharsLength);
                if (match.Success)
                    result.Add(textSpan);
            }
            return result.ToArray();
        }

        public static TextSpan GetTextSpan(this Match match, string text, int escapeCharsLength = 0)
        {
            if (!match.Success || match.Length == 0)
                return TextSpan.Empty;

            int startIndex = match.Index + escapeCharsLength;
            return new TextSpan(startIndex, match.Length);
        }

        public static IEnumerable<MatchResultDto> ToDto(this IEnumerable<MatchResult> matchResults)
        {
            return matchResults
                .Where(result => result != null)
                .Select(result => new MatchResultDto(result));
        }
    }
}
