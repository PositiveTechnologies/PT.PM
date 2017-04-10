using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class LockStatement : SpecificStatement
    {
        public override NodeType NodeType => NodeType.LockStatement;

        public Expression Lock { get; set; }

        public Statement Embedded { get; set; }

        public LockStatement(Expression lockExpression, Statement embedded, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Lock = lockExpression;
            Embedded = embedded;
        }

        public LockStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Lock, Embedded};
        }
    }
}
