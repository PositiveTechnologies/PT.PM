using System.Text.RegularExpressions;

namespace PT.PM.Matching
{
    public class FileNameMatcher
    {
        public bool IsWholeWord { get; set; } = false;

        public bool IsCaseInsensitive { get; set; } = false;

        public Regex WildcardToRegex(string wildcard)
        {
            string regex = wildcard;
            regex = regex.Replace("**", "||");
            regex = regex.Replace("*", "|");
            regex = regex.Replace(".", "\\.");
            regex = regex.Replace("||", ".*");
            regex = regex.Replace("|", "[^/]*");
            if (IsWholeWord)
            {
                regex = $"^({regex})$";
            }
            if (IsCaseInsensitive)
            {
                regex = "(?i)" + regex;
            }
            return new Regex(regex, RegexOptions.Compiled);
        }
    }
}
