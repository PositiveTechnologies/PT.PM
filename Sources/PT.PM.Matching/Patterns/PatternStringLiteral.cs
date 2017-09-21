using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringLiteral : StringLiteral, IRelativeLocationMatching
    {
        public override UstKind Kind => UstKind.PatternStringLiteral;

        [JsonIgnore]
        public Regex Regex { get; set; }

        public TextSpan[] MatchedLocations { get; set; }

        public override string Text
        {
            get
            {
                return Regex.ToString();
            }
            set
            {
                Regex = new Regex(value, RegexOptions.Compiled);
            }
        }

        public PatternStringLiteral(string regexString, TextSpan textSpan)
            : base(regexString, textSpan)
        {
        }

        public PatternStringLiteral(string regexString)
            : base(regexString, default(TextSpan))
        {
        }

        public PatternStringLiteral()
            : base(@".*", default(TextSpan))
        {
        }

        public override string TextValue => Regex.ToString();

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)Kind;
            }

            if (other.Kind == UstKind.PatternStringLiteral)
            {
                return Text.CompareTo(((PatternStringLiteral)other).Text);
            }

            if (other.Kind != UstKind.StringLiteral)
            {
                return Kind - other.Kind;
            }

            MatchedLocations = PatternHelper.MatchRegex(Regex, ((StringLiteral)other).Text, isString: true);
            return MatchedLocations.Length == 0 ? 1 : 0;
        }
    }
}
