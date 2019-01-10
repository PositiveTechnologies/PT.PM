using PT.PM.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public static class PatternUtils
    {
        private static readonly Regex SupressMarkerRegex = new Regex("ptai\\s*:\\s*suppress",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<TextSpan> MatchRegex(this Regex patternRegex, string text, int escapeCharsLength = 0)
        {
            if (patternRegex.ToString() == ".*")
            {
                return new List<TextSpan> { new TextSpan(escapeCharsLength, text.Length) };
            }
            
            MatchCollection matches = patternRegex.Matches(text);
            var result = new List<TextSpan>(matches.Count);

            foreach (Match match in matches)
            {
                TextSpan textSpan = match.GetTextSpan(escapeCharsLength);
                if (match.Success)
                    result.Add(textSpan);
            }

            return result;
        }

        public static TextSpan GetTextSpan(this Match match, int escapeCharsLength = 0)
        {
            if (!match.Success)
            {
                return TextSpan.Zero;
            }

            int startIndex = match.Index + escapeCharsLength;
            return new TextSpan(startIndex, match.Length);
        }

        public static IEnumerable<MatchResultDto> ToDto(this IEnumerable<IMatchResultBase> matchResults)
        {
            return matchResults
                .Where(result => result is MatchResult)
                .Select(result => new MatchResultDto((MatchResult)result));
        }

        public static bool IsSuppressed(CodeFile sourceFile, LineColumnTextSpan lineColumnTextSpan)
        {
            string prevLine = lineColumnTextSpan.BeginLine - 1 > 0
                            ? sourceFile.GetStringAtLine(lineColumnTextSpan.BeginLine - 1)
                            : "";
            return SupressMarkerRegex.IsMatch(prevLine);
        }
    }
}
