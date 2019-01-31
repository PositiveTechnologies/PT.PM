using MessagePack;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class WrapperStatement : Statement
    {
        [Key(UstFieldOffset)]
        public Ust Node { get; set; }

        public WrapperStatement(Ust node)
            : base(node.TextSpan)
        {
            Node = node;
        }

        public WrapperStatement(Ust node, TextSpan textSpan)
            : base(textSpan)
        {
            Node = node;
        }

        public WrapperStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Node };
        }
    }
}
