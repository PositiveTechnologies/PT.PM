using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ReturnStatement : Statement
    {
        public Expression Return { get; set; }

        public ReturnStatement(Expression returnExpression, TextSpan textSpan)
            : base(textSpan)
        {
            Return = returnExpression;
        }

        public ReturnStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Return};
        }

        public override string ToString()
        {
            return $"return {Return};";
        }
    }
}
