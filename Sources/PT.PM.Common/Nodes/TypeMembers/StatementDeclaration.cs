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
        public override NodeType NodeType => NodeType.StatementDeclaration;

        public Statement Statement { get; set; }

        public StatementDeclaration(Statement statement, TextSpan textSpan, RootNode fileNode)
            :base(new IdToken("Statement"), textSpan, fileNode)
        {
            Statement = statement;
        }

        public StatementDeclaration()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(base.GetChildren());
            result.Add(Statement);
            return result.ToArray();
        }
    }
}
