namespace PT.PM.Common.Nodes.Statements
{
    public abstract class Statement : UstNode
    {
        protected Statement(TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected Statement()
        {
        }
    }
}
