using PT.PM.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PT.PM.Common.Files;

namespace PT.PM.Matching
{
    public static class MatchUtils
    {
        private static readonly Regex suppressMarkerRegex = new Regex("ptai\\s*:\\s*suppress",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static List<TextSpan> AlignTextSpans(List<TextSpan> foldedTextSpans, List<TextSpan> matchesLocations, int escapeLength)
        {
            List<TextSpan> result = new List<TextSpan>(matchesLocations.Count);

            int startOffset = foldedTextSpans[0].Start;

            foreach (TextSpan location in matchesLocations)
            {
                int offset = 0;
                int leftBound = 0;
                int rightBound =
                    foldedTextSpans[0].Length;
                TextSpan textSpan = TextSpan.Zero;

                // Check first initial TextSpan separately
                if (location.Start < rightBound && location.End > rightBound)
                {
                    textSpan = location;
                }

                for (int i = 1; i < foldedTextSpans.Count; i++)
                {
                    var initTextSpan = foldedTextSpans[i];
                    var prevTextSpan = foldedTextSpans[i - 1];
                    leftBound += prevTextSpan.Length;
                    rightBound += initTextSpan.Length;
                    offset += initTextSpan.Start - prevTextSpan.End;

                    if (location.Start < leftBound && location.End < leftBound)
                    {
                        break;
                    }

                    if (location.Start >= leftBound && location.Start < rightBound)
                    {
                        textSpan = location.AddOffset(offset);
                        if (location.End <= rightBound)
                        {
                            result.Add(new TextSpan(textSpan.Start + startOffset - escapeLength,
                                textSpan.Length + 2 * escapeLength, textSpan.File));
                            break;
                        }
                    }

                    if (!textSpan.IsZero && location.End <= rightBound)
                    {
                        result.Add(new TextSpan(textSpan.Start + startOffset - escapeLength,
                            location.Length + offset + 2 * escapeLength, textSpan.File));
                        break;
                    }
                }

                if (textSpan.IsZero)
                {
                    result.Add(new TextSpan(textSpan.Start + startOffset - escapeLength,
                        textSpan.Length + 2 * escapeLength, textSpan.File));
                }
            }

            return result;
        }

        public static List<TextSpan> MatchRegex(this Regex patternRegex, TextFile textFile, TextSpan textSpan, int escapeCharsLength)
        {
            return MatchRegex(patternRegex, textFile.Data, escapeCharsLength, textSpan.Start, textSpan.Length, 0);
        }

        public static List<TextSpan> MatchRegex(this Regex patternRegex, string text, int escapeCharsLength, int offset)
        {
            return MatchRegex(patternRegex, text, escapeCharsLength, 0, text.Length, offset);
        }

        private static List<TextSpan> MatchRegex(this Regex patternRegex, string text, int escapeCharsLength, int start, int length, int offset)
        {
            if (patternRegex.ToString() == ".*")
            {
                return new List<TextSpan>
                {
                    SafeCreateTextSpan(start - escapeCharsLength + offset, length + 2 * escapeCharsLength, text)
                };
            }

            int end = start + length;
            if (end > text.Length)
            {
                end = text.Length;
            }

            var result = new List<TextSpan>();

            Match match = patternRegex.Match(text, start, end - start);
            while (match.Success)
            {
                result.Add(match.GetTextSpan(escapeCharsLength, offset, text));

                if (match.Length == 0)
                {
                    break;
                }

                start = match.Index + match.Length;
                match = patternRegex.Match(text, start, end - start);
            }

            return result;
        }

        public static TextSpan GetTextSpan(this Match match, int escapeCharsLength, int offset, string data)
        {
            if (!match.Success)
            {
                return TextSpan.Zero;
            }

            int startIndex = offset + match.Index - escapeCharsLength;
            return SafeCreateTextSpan(startIndex, match.Length + 2 * escapeCharsLength, data);
        }

        public static IEnumerable<MatchResultDto> ToDto(this IEnumerable<IMatchResultBase> matchResults)
        {
            return matchResults
                .Where(result => result is MatchResult)
                .Select(result => new MatchResultDto((MatchResult)result));
        }

        public static bool IsSuppressed(TextFile sourceFile, LineColumnTextSpan lineColumnTextSpan)
        {
            int lineInd = lineColumnTextSpan.BeginLine - 1;

            if (lineInd > 0)
            {
                TextSpan textSpan = sourceFile.GetTextSpanAtLine(lineInd);
                return suppressMarkerRegex.Match(sourceFile.Data, textSpan.Start, textSpan.Length).Success;
            }

            return false;
        }

        public static TextSpan SafeCreateTextSpan(int start, int length, string data)
        {
            if (start < 0)
            {
                start = 0;
            }

            if (start + length > data.Length)
            {
                length = data.Length - start;
            }

            return new TextSpan(start, length);
        }
    }
}
