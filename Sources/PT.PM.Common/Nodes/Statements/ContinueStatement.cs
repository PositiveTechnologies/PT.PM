using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    public class ContinueStatement : Statement
    {
        public override UstKind Kind => UstKind.ContinueStatement;

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
