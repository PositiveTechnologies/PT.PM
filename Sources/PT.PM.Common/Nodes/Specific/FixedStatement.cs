using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public class FixedStatement : SpecificStatement
    {
        public override NodeType NodeType
        {
            get { return NodeType.FixedStatement; }
        }

        public TypeToken Type { get; set; }

        public IEnumerable<AssignmentExpression> Variables { get; set; }

        public Statement Embedded { get; set; }

        public FixedStatement(TypeToken type, IEnumerable<AssignmentExpression> vars, Statement embedded, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Type = type;
            Variables = vars;
            Embedded = embedded;
        }

        public FixedStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(Variables);
            result.Add(Embedded);
            return result.ToArray();
        }
    }
}
