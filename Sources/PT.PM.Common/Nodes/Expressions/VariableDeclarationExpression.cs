using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class VariableDeclarationExpression : Expression
    {
        public override NodeType NodeType => NodeType.VariableDeclarationExpression;

        public TypeToken Type { get; set; }

        public List<AssignmentExpression> Variables { get; set; }

        public VariableDeclarationExpression(TypeToken type, IEnumerable<AssignmentExpression> variables,
            TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            Variables = variables as List<AssignmentExpression> ?? variables.ToList();
        }

        public VariableDeclarationExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Type);
            result.AddRange(Variables);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{Type} {string.Join(", ", Variables)}";
        }
    }
}
