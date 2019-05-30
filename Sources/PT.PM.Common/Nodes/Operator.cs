using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class Operator : Ust, ITerminal, IOperatorOrPunctuator
    {
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