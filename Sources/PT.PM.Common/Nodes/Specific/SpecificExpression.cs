using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public abstract class SpecificExpression : Expression
    {
        protected SpecificExpression(TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected SpecificExpression()
        {
        }
    }
}
