using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public interface IRegexPattern
    {
        string Default { get; }

        string RegexString { get; set; }

        Regex Regex { get; }
    }
}
