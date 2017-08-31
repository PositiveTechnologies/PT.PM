using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Nodes.Statements
{
    public class GotoStatement : Statement
    {
        public override NodeType NodeType => NodeType.GotoStatement;

        public Expression Id { get; set; }

        public GotoStatement()
        {
        }

        public GotoStatement(Expression id, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            Id = id;
        }

        public override UstNode[] GetChildren()
        {
            return new UstNode[] { Id };
        }

        public override string ToString()
        {
            return $"goto {Id};";
        }
    }
}
