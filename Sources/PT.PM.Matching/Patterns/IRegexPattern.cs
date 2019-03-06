using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public interface IRegexPattern
    {
        [JsonIgnore]
        string Default { get; }

        string RegexString { get; set; }

        [JsonIgnore]
        Regex Regex { get; }
    }
}
