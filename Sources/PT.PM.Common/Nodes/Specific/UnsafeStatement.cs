using MessagePack;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class UnsafeStatement : SpecificStatement
    {
        [Key(0)] public override UstType UstType => UstType.UnsafeStatement;

        [Key(UstFieldOffset)]
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

        public override string ToString()
        {
            return $"unsafe\n{{\n{Body.ToStringWithTrailNL()}}}";
        }
    }
}
