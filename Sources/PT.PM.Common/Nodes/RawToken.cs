using System;
using MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class RawToken : Ust, ITerminal
    {
        [Key(0)] public override UstType UstType => UstType.RawToken;

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
