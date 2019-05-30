using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class Comment : Ust
    {
        public Comment(TextSpan textSpan, RootUst rootUst)
            : base(textSpan, rootUst)
        {
        }

        public Comment()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}
