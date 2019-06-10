using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class CommaExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.CommaExpression;

        [Key(UstFieldOffset)]
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
