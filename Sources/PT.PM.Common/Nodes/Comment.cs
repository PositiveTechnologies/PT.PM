using MessagePack;

namespace PT.PM.Common.Nodes.Tokens.Literals
{
    [MessagePackObject]
    public class Comment : Ust
    {
        public Comment(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public Comment()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;
    }
}
