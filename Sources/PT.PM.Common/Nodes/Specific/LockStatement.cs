using MessagePack;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class LockStatement : SpecificStatement
    {
        [Key(UstFieldOffset)]
        public Expression Lock { get; set; }

        [Key(UstFieldOffset + 1)]
        public Statement Embedded { get; set; }

        public LockStatement(Expression lockExpression, Statement embedded, TextSpan textSpan)
            : base(textSpan)
        {
            Lock = lockExpression;
            Embedded = embedded;
        }

        public LockStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Lock, Embedded};
        }

        public override string ToString()
        {
            return $"lock ({Lock})\n{Embedded}";
        }
    }
}
