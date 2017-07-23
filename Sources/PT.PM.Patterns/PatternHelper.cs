using PT.PM.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns
{
    public class PatternHelper
    {
        public static TextSpan[] MatchRegex(Regex patternRegex, string text, bool returnSingle = false, bool isString = false)
        {
            if (returnSingle)
            {
                Match match = patternRegex.Match(text);
                if (match.Success)
                {
                    int startIndex = isString ? match.Index + 1 : match.Index; // TODO: Fix location in UST node.
                    return new TextSpan[] { new TextSpan(startIndex, match.Length) };
                }
                else
                {
                    return new TextSpan[0];
                }
            }
            else
            {
                MatchCollection matches = patternRegex.Matches(text);
                var result = new List<TextSpan>(matches.Count);
                foreach (Match match in matches)
                {
                    int startIndex = isString ? match.Index + 1 : match.Index;  // TODO: Fix location in UST node.
                    result.Add(new TextSpan(startIndex, match.Length));
                }
                return result.ToArray();
            }
        }
    }
}
