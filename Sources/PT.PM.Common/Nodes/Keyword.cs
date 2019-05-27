using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class Keyword : Ust, ITerminal
    {
        public Keyword(TextSpan textSpan, RootUst rootUst)
            : base(textSpan)
        {
            Root = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
        }

        public Keyword()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}