namespace PT.PM.Common.Nodes.Statements
{
    public class EmptyStatement : Statement
    {
        public EmptyStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public EmptyStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }

        public override string ToString()
        {
            return ";";
        }
    }
}
