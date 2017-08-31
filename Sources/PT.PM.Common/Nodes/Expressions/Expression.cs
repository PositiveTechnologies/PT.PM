namespace PT.PM.Common.Nodes.Expressions
{
    public abstract class Expression : UstNode
    {
        protected Expression(TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected Expression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Expression()
        {
        }
    }
}
