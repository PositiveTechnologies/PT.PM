using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    public abstract class SpecificExpression : Expression
    {
        protected SpecificExpression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected SpecificExpression()
        {
        }
    }
}
