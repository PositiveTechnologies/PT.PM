using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class CheckedStatement : SpecificStatement
    {
        public override NodeType NodeType => NodeType.CheckedStatement;

        public BlockStatement Body { get; set; }

        public CheckedStatement(BlockStatement body, TextSpan textSpan)
            : base(textSpan)
        {
            Body = body;
        }

        public CheckedStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] {Body};
        }
    }
}
