using MessagePack;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public abstract class SpecificStatement : Statement
    {
        protected SpecificStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected SpecificStatement()
        {
        }
    }
}
