using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    [Union((int)NodeType.AsExpression, typeof(AsExpression))]
    [Union((int)NodeType.CheckedExpression, typeof(CheckedExpression))]
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
