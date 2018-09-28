using PT.PM.Common.Nodes.Statements;
using System;

namespace PT.PM.Common.Nodes.Specific
{
    public class WithStatement : Statement
    {
        public Ust Expression { get; set; }

        public Statement Statement { get; set; }

        public WithStatement(Ust expression, Statement statement, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        public WithStatement()
        {
        }

        public override Ust[] GetChildren() => new Ust[] { Expression, Statement };

        public override string ToString()
        {
            return $"with ({Expression})\n{Statement}";
        }
    }
}
