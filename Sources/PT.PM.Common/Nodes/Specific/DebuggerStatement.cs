using MessagePack;
using PT.PM.Common.Nodes.Statements;

namespace PT.PM.Common.Nodes.Specific
{
    [MessagePackObject]
    public class DebuggerStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.DebuggerStatement;

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
