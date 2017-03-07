using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class AnonymousMethodExpression : Expression
    {
        public override NodeType NodeType => NodeType.AnonymousMethodExpression;

        public IEnumerable<ParameterDeclaration> Parameters { get; set; } = ArrayUtils<ParameterDeclaration>.EmptyArray;

        public BlockStatement Body { get; set; }

        public AnonymousMethodExpression(IEnumerable<ParameterDeclaration> parameters, BlockStatement body,
            TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Parameters = parameters;
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
