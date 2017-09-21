using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class DoWhileStatement : Statement
    {
        public override UstKind Kind => UstKind.DoWhileStatement;

        public Statement EmbeddedStatement { get; set; }

        public Expression Condition { get; set; }

        public DoWhileStatement(Statement embeddedStatement, Expression condition, TextSpan textSpan)
            : base(textSpan)
        {
            EmbeddedStatement = embeddedStatement;
            Condition = condition;
        }

        public DoWhileStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust> {EmbeddedStatement, Condition};
            return result.ToArray();
        }
    }
}
