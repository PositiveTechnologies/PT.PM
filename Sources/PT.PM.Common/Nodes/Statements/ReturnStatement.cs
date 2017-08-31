using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ReturnStatement : Statement
    {
        public override NodeType NodeType => NodeType.ReturnStatement;

        public Expression Return { get; set; }

        public ReturnStatement(Expression returnExpression, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Return = returnExpression;
        }

        public ReturnStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Return};
        }

        public override string ToString()
        {
            return $"return {Return};";
        }
    }
}
