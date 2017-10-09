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
        public Expression Id { get; set; }

        public GotoStatement()
        {
        }

        public GotoStatement(Expression id, TextSpan textSpan)
            : base(textSpan)
        {
            Id = id;
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Id };
        }

        public override string ToString()
        {
            return $"goto {Id};";
        }
    }
}
