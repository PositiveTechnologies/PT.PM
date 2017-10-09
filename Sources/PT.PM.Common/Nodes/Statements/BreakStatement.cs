using PT.PM.Common.Nodes.Expressions;
namespace PT.PM.Common.Nodes.Statements
{
    public class BreakStatement : Statement
    {
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
