using System;
using MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class Punctuator : Ust, ITerminal, IOperatorOrPunctuator
    {
        [Key(0)] public override UstType UstType => UstType.Punctuator;

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
