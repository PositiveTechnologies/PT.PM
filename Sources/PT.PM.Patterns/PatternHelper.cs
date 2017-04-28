using PT.PM.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns
{
    public class PatternHelper
    {
        public static TextSpan[] MatchRegex(Regex patternRegex, string text, bool single = false)
        {
            if (single)
            {
                Match match = patternRegex.Match(text);
                if (match.Success)
                {
                    return new TextSpan[] { new TextSpan(match.Index, match.Length) };
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
                    result.Add(new TextSpan(match.Index, match.Length));
                }
                return result.ToArray();
            }
        }
    }
}
