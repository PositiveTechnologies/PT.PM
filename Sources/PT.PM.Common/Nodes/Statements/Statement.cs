namespace PT.PM.Common.Nodes.Statements
{
    public abstract class Statement : UstNode
    {
        protected Statement(TextSpan textSpan, RootNode root)
            : base(textSpan, root)
        {
        }

        protected Statement()
        {
        }
    }
}
