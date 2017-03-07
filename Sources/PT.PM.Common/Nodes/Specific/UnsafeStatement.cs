using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class UnsafeStatement : SpecificStatement
    {
        public override NodeType NodeType
        {
            get { return NodeType.UnsafeStatement; }
        }

        public BlockStatement Body { get; set; }

        public UnsafeStatement(BlockStatement body, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Body = body;
        }

        public UnsafeStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Body};
        }
    }
}
