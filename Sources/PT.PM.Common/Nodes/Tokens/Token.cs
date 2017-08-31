using PT.PM.Common.Nodes.Expressions;
using Newtonsoft.Json;

namespace PT.PM.Common.Nodes.Tokens
{
    public abstract class Token : Expression
    {
        [JsonIgnore]
        public abstract string TextValue { get; }

        public override bool IsLiteral => true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Expression Expression { get; set; }

        protected Token(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Token()
        {
        }

        public sealed override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override int CompareTo(UstNode other)
        {
            if (other == null)
            {
                return 1;
            }

            if (!other.IsLiteral)
            {
                return -1;
            }

            var nodeTypeResult = NodeType - other.NodeType;
            if (nodeTypeResult != 0)
            {
                return nodeTypeResult;
            }

            Token otherLiteral = (Token)other;
            if (Expression != null && otherLiteral.Expression == null)
            {
                return 1;
            }

            if (Expression == null && otherLiteral.Expression != null)
            {
                return -1;
            }

            if (Expression != null && otherLiteral.Expression != null)
            {
                var expressionCompareResult = Expression.CompareTo(otherLiteral.Expression);
                if (expressionCompareResult != 0)
                    return expressionCompareResult;
            }

            return 0;
        }

        public override string ToString()
        {
            return TextValue;
        }
    }
}
