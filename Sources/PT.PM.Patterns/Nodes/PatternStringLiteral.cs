using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Text.RegularExpressions;

namespace PT.PM.Patterns.Nodes
{
    public class PatternStringLiteral : StringLiteral
    {
        public override NodeType NodeType => NodeType.PatternStringLiteral;

        [JsonIgnore]
        public Regex Regex { get; set; }

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
            : base(@"\w*", default(TextSpan), null)
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

            int result = Regex.IsMatch(((StringLiteral) other).Text) ? 0 : 1;
            return result;
        }
    }
}
