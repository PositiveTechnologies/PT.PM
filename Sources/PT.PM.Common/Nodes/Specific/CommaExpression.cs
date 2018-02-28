using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Specific
{
    public class CommaExpression : Expression
    {
        public List<Expression> Expressions { get; set; }

        public CommaExpression(IEnumerable<Expression> expressions)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            Expressions = expressions.ToList();
        }

        public CommaExpression()
        {
            Expressions = new List<Expression>();
        } 

        public override Expression[] GetArgs() => Expressions.ToArray();

        public override Ust[] GetChildren() => Expressions.ToArray();

        public override string ToString() => $"{string.Join(", ", Expressions)}";
    }
}
