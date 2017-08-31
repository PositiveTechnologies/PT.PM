namespace PT.PM.Common.Nodes.Statements
{
    public class EmptyStatement : Statement
    {
        public override NodeType NodeType => NodeType.EmptyStatement;

        public EmptyStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public EmptyStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return ArrayUtils<UstNode>.EmptyArray;
        }

        public override string ToString()
        {
            return ";";
        }
    }
}
