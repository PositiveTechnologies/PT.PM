using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public abstract class SpecificStatement : Statement
    {
        protected SpecificStatement(TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
        }

        protected SpecificStatement()
        {
        }
    }
}
