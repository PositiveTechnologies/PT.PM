using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class StatementDeclaration : EntityDeclaration
    {
        public Statement Statement { get; set; }

        public StatementDeclaration(Statement statement, TextSpan textSpan)
            :base(new IdToken("Statement"), textSpan)
        {
            Statement = statement;
        }

        public StatementDeclaration()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>(base.GetChildren());
            result.Add(Statement);
            return result.ToArray();
        }
    }
}
