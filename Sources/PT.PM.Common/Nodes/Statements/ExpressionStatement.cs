using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }

        public ExpressionStatement()
        {
        }

        public ExpressionStatement(Expression expression, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = expression;
            TextSpan = textSpan.IsZero ? expression.TextSpan : textSpan;
        }

        public override Ust[] GetChildren() => new Ust[] { Expression };

        public override string ToString()
        {
            return $"{Expression};" + System.Environment.NewLine;
        }
    }
}
