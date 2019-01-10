using MessagePack;
using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class ContinueStatement : Statement
    {
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
