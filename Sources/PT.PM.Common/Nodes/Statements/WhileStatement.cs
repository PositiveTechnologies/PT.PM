using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System;

namespace PT.PM.Common.Nodes.Statements
{
    public class WhileStatement : Statement
    {
        public Expression Condition { get; set; }

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
