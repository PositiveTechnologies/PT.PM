using PT.PM.Common.Nodes.Statements;
using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class WithStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.WithStatement;

        [Key(UstFieldOffset)]
        public Ust Expression { get; set; }

        [Key(UstFieldOffset + 1)]
        public Statement Statement { get; set; }

        public WithStatement(Ust expression, Statement statement, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        public WithStatement()
        {
        }

        public override Ust[] GetChildren() => new[] { Expression, Statement };

        public override string ToString()
        {
            return $"with ({Expression})\n{Statement}";
        }
    }
}
