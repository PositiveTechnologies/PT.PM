using MessagePack;

namespace PT.PM.Common.Nodes
{
    [MessagePackObject]
    public class Comment : Ust
    {
        [Key(0)] public override UstType UstType => UstType.Comment;

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
