using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public class CheckedExpression : SpecificExpression
    {
        public override NodeType NodeType
        {
            get { return NodeType.CheckedExpression; }
        }

        public Expression Expression { get; set; }

        public CheckedExpression(Expression checkedExpression, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Expression = checkedExpression;
            TextSpan = textSpan;
            FileNode = fileNode;
        }

        public CheckedExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Expression};
        }
    }
}
