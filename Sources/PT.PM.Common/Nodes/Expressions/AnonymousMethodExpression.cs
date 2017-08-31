using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class AnonymousMethodExpression : Expression
    {
        public override NodeType NodeType => NodeType.AnonymousMethodExpression;

        public List<ParameterDeclaration> Parameters { get; set; } = new List<ParameterDeclaration>();

        public BlockStatement Body { get; set; }

        public AnonymousMethodExpression(IEnumerable<ParameterDeclaration> parameters, BlockStatement body,
            TextSpan textSpan)
            : base(textSpan)
        {
            Parameters = parameters as List<ParameterDeclaration> ?? parameters.ToList();
            Body = body;
        }

        public AnonymousMethodExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
