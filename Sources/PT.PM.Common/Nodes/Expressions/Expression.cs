using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    [Union((int)NodeType.BigIntLiteral, typeof(BigIntLiteral))]
    [Union((int)NodeType.LongLiteral, typeof(LongLiteral))]
    public abstract class Expression : Ust
    {
        protected Expression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Expression()
        {
        }

        public abstract Expression[] GetArgs();
    }
}
