using System;
using MessagePack;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class RawToken : Ust, ITerminal
    {
        public RawToken(TextSpan textSpan, RootUst rootUst)
            : base(textSpan)
        {
            Root = rootUst ?? throw new ArgumentNullException(nameof(rootUst));
        }

        public RawToken()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}