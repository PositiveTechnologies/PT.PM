namespace PT.PM.Common.Nodes.Statements
{
    public abstract class Statement : UstNode
    {
        protected Statement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Statement()
        {
        }
    }
}
