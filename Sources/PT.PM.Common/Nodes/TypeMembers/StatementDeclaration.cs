using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Nodes.TypeMembers
{
    public class StatementDeclaration : EntityDeclaration
    {
        public override UstKind Kind => UstKind.StatementDeclaration;

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
