using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class WhileStatement : Statement
    {
        [Key(UstFieldOffset)]
        public Expression Condition { get; set; }

        [Key(UstFieldOffset + 1)]
        public Statement Embedded { get; set; }

        public WhileStatement(Expression condition, Statement embedded, TextSpan textSpan)
            : base(textSpan)
        {
            Condition = condition;
            Embedded = embedded;
        }

        public WhileStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Condition);
            result.Add(Embedded);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"while ({Condition})\n{Embedded}";
        }
    }
}
