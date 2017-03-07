using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    public class ContinueStatement : Statement
    {
        public override NodeType NodeType => NodeType.ContinueStatement;

        public Expression Expression { get; set; }

        public ContinueStatement(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public ContinueStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override string ToString()
        {
            return "continue;";
        }
    }
}
