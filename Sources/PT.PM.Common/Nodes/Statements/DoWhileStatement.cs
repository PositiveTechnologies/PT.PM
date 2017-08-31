using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class DoWhileStatement : Statement
    {
        public override NodeType NodeType => NodeType.DoWhileStatement;

        public Statement EmbeddedStatement { get; set; }

        public Expression Condition { get; set; }

        public DoWhileStatement(Statement embeddedStatement, Expression condition, TextSpan textSpan, RootNode root)
            : base(textSpan, root)
        {
            EmbeddedStatement = embeddedStatement;
            Condition = condition;
        }

        public DoWhileStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode> {EmbeddedStatement, Condition};
            return result.ToArray();
        }
    }
}
