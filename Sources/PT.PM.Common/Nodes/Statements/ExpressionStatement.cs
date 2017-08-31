using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ExpressionStatement : Statement
    {
        public override NodeType NodeType => NodeType.ExpressionStatement;

        public Expression Expression { get; set; }

        public ExpressionStatement(Expression expression, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
        }

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
            TextSpan = expression.TextSpan;
        }

        public ExpressionStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Expression};
        }

        public override string ToString()
        {
            return $"{Expression};" + System.Environment.NewLine;
        }
    }
}
