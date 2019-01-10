using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ExpressionStatement : Statement
    {
        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        public ExpressionStatement()
        {
        }

        public ExpressionStatement(Expression expression, TextSpan textSpan = default)
            : base(textSpan)
        {
            Expression = expression;
            TextSpan = textSpan.IsZero ? expression.TextSpan : textSpan;
        }

        public override Ust[] GetChildren() => new Ust[] { Expression };

        public override string ToString()
        {
            return $"{Expression};";
        }
    }
}
