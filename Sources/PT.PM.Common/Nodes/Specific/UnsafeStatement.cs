using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class UnsafeStatement : SpecificStatement
    {
        public override UstKind Kind => UstKind.UnsafeStatement;

        public BlockStatement Body { get; set; }

        public UnsafeStatement(BlockStatement body, TextSpan textSpan)
            : base(textSpan)
        {
            Body = body;
        }

        public UnsafeStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {Body};
        }
    }
}
