using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Common.Nodes.Specific
{
    public class AsExpression : SpecificExpression
    {
        public override NodeType NodeType => NodeType.AsExpression;

        public Expression Expression { get; set; }

        public TypeToken Type { get; set; }

        public AsExpression(Expression expression, TypeToken type, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Expression = expression;
            Type = type;
        }

        public AsExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode> { Expression, Type };
            return result.ToArray();
        }
    }
}
