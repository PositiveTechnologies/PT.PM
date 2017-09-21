using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System;

namespace PT.PM.Common.Nodes.Statements
{
    public class IfElseStatement : Statement
    {
        public override UstKind Kind => UstKind.IfElseStatement;

        public Expression Condition { get; set; }

        public Statement TrueStatement { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public Statement FalseStatement { get; set; }

        public IfElseStatement(Expression condition, Statement trueStatement, TextSpan textSpan)
            : base(textSpan)
        {
            Condition = condition;
            TrueStatement = trueStatement;
        }

        public IfElseStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>()
            {
                Condition,
                TrueStatement
            };
            if (FalseStatement != null)
            {
                result.Add(FalseStatement);
            }

            return result.ToArray();
        }

        public override string ToString()
        {
            string nl = Environment.NewLine;
            var result = $"if ({Condition}){nl}{{{nl}  {TrueStatement}{nl}}}";
            if (FalseStatement != null)
            {
                result += $"else{nl}{{{nl}  {FalseStatement}{nl}}}";
            }
            return result;
        }
    }
}
