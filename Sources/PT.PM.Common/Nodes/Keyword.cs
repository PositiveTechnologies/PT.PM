using System;
using MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class Keyword : Ust, ITerminal
    {
        [Key(0)] public override UstType UstType => UstType.Keyword;

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
