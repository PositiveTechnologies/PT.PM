using System.Collections.Generic;
using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class DoWhileStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.DoWhileStatement;

        [Key(UstFieldOffset)]
        public Statement EmbeddedStatement { get; set; }

        [Key(UstFieldOffset + 1)]
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

        public override string ToString()
        {
            return $"do\n{EmbeddedStatement.ToStringWithTrailNL()}while ({Condition})";
        }
    }
}
