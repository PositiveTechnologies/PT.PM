using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ThrowStatement : Statement
    {
        public override NodeType NodeType => NodeType.ThrowStatement;

        public Expression ThrowExpression { get; set; }

        public ThrowStatement(Expression throwExpression, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            ThrowExpression = throwExpression;
        }

        public ThrowStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {ThrowExpression};
        }

        public override string ToString()
        {
            return $"throw {ThrowExpression};";
        }
    }
}
