using System.Text.RegularExpressions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using Newtonsoft.Json;

namespace PT.PM.Patterns.Nodes
{
    public class PatternIdToken : IdToken
    {
        public override NodeType NodeType => NodeType.PatternIdToken;

        [JsonIgnore]
        public Regex Regex { get; set; }

        public override string Id
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

        public PatternIdToken(string regexId, TextSpan textSpan)
            : base(string.IsNullOrEmpty(regexId) ? @"\w+" : regexId, textSpan, null)
        {
        }

        public PatternIdToken(string regexId)
            : base(string.IsNullOrEmpty(regexId) ? @"\w+" : regexId, default(TextSpan), null)
        {
        }

        public PatternIdToken()
            : base(@"\w+", default(TextSpan), null)
        {
        }

        public override string TextValue
        {
            get { return Regex.ToString(); }
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other.NodeType == NodeType.PatternIdToken)
            {
                return Id.CompareTo(((PatternIdToken)other).Id);
            }

            if (other.NodeType != NodeType.PatternIdToken && other.NodeType != NodeType.IdToken)
            {
                return NodeType - other.NodeType;
            }

            var match = Regex.IsMatch(((IdToken)other).Id);
            return match ? 0 : 1;
        }
    }
}
