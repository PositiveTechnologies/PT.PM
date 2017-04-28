using PT.PM.Common;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns
{
    public class PatternHelper
    {
        public static TextSpan MatchRegex(Regex patternRegex, string text)
        {
            Match match = patternRegex.Match(text);
            return match.Success ? new TextSpan(match.Index, match.Length) : default(TextSpan);
        }
    }
}
