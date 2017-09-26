using PT.PM.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PT.PM.Matching
{
    public static class PatternUtils
    {
        public static TextSpan[] MatchRegex(this Regex patternRegex, string text, bool returnSingle = false, bool isQuoted = false)
        {
            if (returnSingle)
            {
                Match match = patternRegex.Match(text);
                var textSpan = match.GetTextSpan(isQuoted);
                if (!textSpan.IsEmpty)
                {
                    return new TextSpan[] { textSpan };
                }
                else
                {
                    return ArrayUtils<TextSpan>.EmptyArray;
                }
            }
            else
            {
                MatchCollection matches = patternRegex.Matches(text);
                var result = new List<TextSpan>(matches.Count);
                foreach (Match match in matches)
                {
                    var textSpan = match.GetTextSpan(isQuoted);
                    if (!textSpan.IsEmpty)
                        result.Add(textSpan);
                }
                return result.ToArray();
            }
        }

        public static TextSpan GetTextSpan(this Match match, bool isQuoted)
        {
            if (!match.Success || match.Length == 0)
                return TextSpan.Empty;

            int startIndex = match.Index;
            if (isQuoted)
                startIndex += 1; // TODO: Fix location in UST node.
            return new TextSpan(startIndex, match.Length);
        }
    }
}
