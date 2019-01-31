using MessagePack;
using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class BreakStatement : Statement
    {
        [Key(UstFieldOffset)]
        public Expression Expression  { get; set; }

        public BreakStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public BreakStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }

        public override string ToString()
        {
            return "break;";
        }
    }
}
