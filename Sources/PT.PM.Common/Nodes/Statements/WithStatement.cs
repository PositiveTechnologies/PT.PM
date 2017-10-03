using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Nodes.Statements
{
    public class WithStatement : Statement
    {
        public Ust WithNode { get; set; }

        public Statement Body { get; set; }

        public WithStatement(Ust withNode, Statement body, TextSpan textSpan)
            : base(textSpan)
        {
            WithNode = withNode;
            Body = body;
        }

        public WithStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(WithNode);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
