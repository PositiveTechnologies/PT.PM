using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    public class BreakStatement : Statement
    {
        public override NodeType NodeType => NodeType.BreakStatement;

        public Expression Expression  { get; set; }

        public BreakStatement(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        public BreakStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override string ToString()
        {
            return "break;";
        }
    }
}
