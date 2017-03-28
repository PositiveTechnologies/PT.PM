using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class FieldDeclaration : EntityDeclaration
    {
        public override NodeType NodeType => NodeType.FieldDeclaration;

        public List<AssignmentExpression> Variables { get; set; }

        public FieldDeclaration(IEnumerable<AssignmentExpression> variables, TextSpan textSpan, FileNode fileNode)
            : base(null, textSpan, fileNode)
        {
            Variables = variables as List<AssignmentExpression> ?? variables.ToList();
        }

        public FieldDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(base.GetChildren());
            result.AddRange(Variables);
            return result.ToArray();
        }
    }
}
