using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Specific
{
    public class CommaExpression : Expression
    {
        public List<Expression> Expressions { get; set; } = new List<Expression>();

        public CommaExpression(IEnumerable<Expression> expressions, TextSpan textSpan)
            : base(textSpan)
        {
            Expressions = expressions as List<Expression> ?? expressions.ToList();
        }

        public CommaExpression()
        {
        } 

        public override Expression[] GetArgs() => Expressions.ToArray();

        public override Ust[] GetChildren() => Expressions.ToArray();

        public override string ToString() => $"{string.Join(", ", Expressions)}";
    }
}
