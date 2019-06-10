using MessagePack;
using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ContinueStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.ContinueStatement;

        [Key(UstFieldOffset)]
        public Expression Expression { get; set; }

        public ContinueStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public ContinueStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }

        public override string ToString()
        {
            return "continue;";
        }
    }
}
