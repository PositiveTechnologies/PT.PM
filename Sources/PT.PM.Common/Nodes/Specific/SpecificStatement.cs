using MessagePack;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    [Union((int)NodeType.FixedStatement, typeof(FixedStatement))]
    [Union((int)NodeType.LockStatement, typeof(LockStatement))]
    [Union((int)NodeType.UnsafeStatement, typeof(UnsafeStatement))]
    public abstract class SpecificStatement : Statement
    {
        protected SpecificStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected SpecificStatement()
        {
        }
    }
}
