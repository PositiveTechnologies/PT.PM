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
        public override NodeType NodeType => NodeType.WithStatement;

        public UstNode WithNode { get; set; }

        public Statement Body { get; set; }

        public WithStatement(UstNode withNode, Statement body, TextSpan textSpan, RootNode fileNode)
            : base(textSpan, fileNode)
        {
            WithNode = withNode;
            Body = body;
        }

        public WithStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(WithNode);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
