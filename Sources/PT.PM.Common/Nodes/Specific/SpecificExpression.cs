using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
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
