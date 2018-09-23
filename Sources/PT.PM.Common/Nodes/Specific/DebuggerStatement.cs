using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    public class DebuggerStatement : Statement
    {
        public DebuggerStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public DebuggerStatement()
        {
        }

        public override Ust[] GetChildren() => new Ust[0];

        public override string ToString() => "debugger;";
    }
}
