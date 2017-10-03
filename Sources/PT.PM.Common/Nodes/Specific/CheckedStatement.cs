using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class CheckedStatement : SpecificStatement
    {
        public BlockStatement Body { get; set; }

        public CheckedStatement(BlockStatement body, TextSpan textSpan)
            : base(textSpan)
        {
            Body = body;
        }

        public CheckedStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Body};
        }
    }
}
