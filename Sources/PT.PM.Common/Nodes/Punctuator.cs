using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class Punctuator : Ust, ITerminal
    {
        public Punctuator(TextSpan textSpan, RootUst rootUst)
            : base(textSpan)
        {
            Root = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
        }

        public Punctuator()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}