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

        public static List<TextSpan> MatchRegex(this Regex patternRegex, TextFile textFile, TextSpan textSpan, int escapeCharsLength = 0)
        {
            return MatchRegex(patternRegex, textFile.Data, textSpan.Start, textSpan.Length, escapeCharsLength);
        }

        public static List<TextSpan> MatchRegex(this Regex patternRegex, string text, int start = 0, int length = -1, int escapeCharsLength = 0)
        {
            if (patternRegex.ToString() == ".*")
            {
                return new List<TextSpan> { new TextSpan(start + escapeCharsLength, length == -1 ? text.Length : length) };
            }

            int end;
            if (length == -1)
            {
                end = text.Length;
            }
            else
            {
                end = start + length;
                if (end > text.Length)
                {
                    end = text.Length;
                }
            }

            var result = new List<TextSpan>();

            Match match = patternRegex.Match(text, start, end - start);
            while (match.Success)
            {
                result.Add(match.GetTextSpan(escapeCharsLength));

                if (match.Length == 0)
                {
                    break;
                }

                start = match.Index + match.Length;
                match = patternRegex.Match(text, start, end - start);
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

        public static bool IsSuppressed(TextFile sourceFile, LineColumnTextSpan lineColumnTextSpan)
        {
            string prevLine = lineColumnTextSpan.BeginLine - 1 > 0
                            ? sourceFile.GetStringAtLine(lineColumnTextSpan.BeginLine - 1)
                            : "";
            return SupressMarkerRegex.IsMatch(prevLine);
        }
    }
}
