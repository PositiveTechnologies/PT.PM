using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System;

namespace PT.PM.Common.Nodes.Statements
{
    public class WhileStatement : Statement
    {
        public override NodeType NodeType => NodeType.WhileStatement;

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

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Condition);
            result.Add(Embedded);
            return result.ToArray();
        }

        public override string ToString()
        {
            string nl = Environment.NewLine;
            return $"while ({Condition}){nl}  {Embedded}{nl}";
        }
    }
}
