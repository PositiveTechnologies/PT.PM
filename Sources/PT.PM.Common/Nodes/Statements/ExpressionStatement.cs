using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ExpressionStatement : Statement
    {
        public override UstKind Kind => UstKind.ExpressionStatement;

        public Expression Expression { get; set; }

        public ExpressionStatement()
        {
        }

        public ExpressionStatement(Expression expression, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Expression = expression;
            TextSpan = textSpan == TextSpan.Empty ? expression.TextSpan : textSpan;
        }

        public override Ust[] GetChildren() => new Ust[] { Expression };

        public override string ToString()
        {
            return $"{Expression};" + System.Environment.NewLine;
        }
    }
}
