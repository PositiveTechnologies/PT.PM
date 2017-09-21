using System.Text.RegularExpressions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Newtonsoft.Json;

namespace PT.PM.Matching.Patterns
{
    public class PatternIdToken : IdToken, IRelativeLocationMatching
    {
        public override UstKind Kind => UstKind.PatternIdToken;

        [JsonIgnore]
        public Regex Regex { get; set; }

        [JsonIgnore]
        public Regex CaseInsensitiveRegex { get; set; }

        public TextSpan[] MatchedLocations { get; set; }

        public override string Id
        {
            get
            {
                return Regex.ToString();
            }
            set
            {
                Regex = new Regex(value, RegexOptions.Compiled);
                CaseInsensitiveRegex = value.StartsWith("(?i)") ? Regex : new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        public PatternIdToken(string regexId, TextSpan textSpan)
            : base(string.IsNullOrEmpty(regexId) ? @"\w+" : regexId, textSpan)
        {
        }

        public PatternIdToken(string regexId)
            : base(string.IsNullOrEmpty(regexId) ? @"\w+" : regexId, default(TextSpan))
        {
        }

        public PatternIdToken()
            : base(@"\w+", default(TextSpan))
        {
        }

        public override string TextValue => Regex.ToString();

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other.Kind == UstKind.PatternIdToken)
            {
                return Id.CompareTo(((PatternIdToken)other).Id);
            }

            if (!typeof(Token).IsAssignableFrom(other.GetType()))
            {
                return Kind - other.Kind;
            }

            var regex = other.Root.Language.IsCaseInsensitive()
                ? CaseInsensitiveRegex
                : Regex;
            MatchedLocations = PatternHelper.MatchRegex(regex, ((Token)other).TextValue, true);
            return MatchedLocations.Length == 0 ? 1 : 0;
        }
    }
}
