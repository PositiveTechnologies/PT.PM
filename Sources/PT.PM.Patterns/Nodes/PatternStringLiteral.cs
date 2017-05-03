using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternStringLiteral : StringLiteral, IRelativeLocationMatching
    {
        public override NodeType NodeType => NodeType.PatternStringLiteral;

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
            : base(regexString, textSpan, null)
        {
        }

        public PatternStringLiteral(string regexString)
            : base(regexString, default(TextSpan), null)
        {
        }

        public PatternStringLiteral()
            : base(@".*", default(TextSpan), null)
        {
        }

        public override string TextValue => Regex.ToString();

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return (int)NodeType;
            }

            if (other.NodeType == NodeType.PatternStringLiteral)
            {
                return Text.CompareTo(((PatternStringLiteral)other).Text);
            }

            if (other.NodeType != NodeType.StringLiteral)
            {
                return NodeType - other.NodeType;
            }

            MatchedLocations = PatternHelper.MatchRegex(Regex, ((StringLiteral)other).Text);
            return MatchedLocations.Length == 0 ? 1 : 0;
        }
    }
}
