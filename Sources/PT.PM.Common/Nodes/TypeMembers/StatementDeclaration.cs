using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.TypeMembers
{
    [MessagePackObject]
    public class StatementDeclaration : EntityDeclaration
    {
        [Key(EntityFieldOffset)]
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
