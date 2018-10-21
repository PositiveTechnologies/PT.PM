using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Sql
{
    public class SqlBlockStatement : Statement
    {
        public IdToken BlockName { get; set; }

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
