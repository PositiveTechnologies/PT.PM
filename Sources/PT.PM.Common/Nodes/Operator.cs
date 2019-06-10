using System;
using MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class Operator : Ust, ITerminal, IOperatorOrPunctuator
    {
        [Key(0)] public override UstType UstType => UstType.Operator;

        public Operator(TextSpan textSpan, RootUst rootUst)
            : base(textSpan)
        {
            Root = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
        }

        public Operator()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}
