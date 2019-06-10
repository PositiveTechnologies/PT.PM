using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Sql
{
    [MessagePackObject]
    public class SqlBlockStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.SqlBlockStatement;

        [Key(UstFieldOffset)]
        public IdToken BlockName { get; set; }

        [Key(UstFieldOffset + 1)]
        public List<Statement> Statements { get; set; }

        public SqlBlockStatement(IdToken blockName, List<Statement> statements, TextSpan textSpan)
            : base(textSpan)
        {
            BlockName = blockName;
            Statements = statements;
        }

        public SqlBlockStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return Statements.ToArray();
        }
    }
}
