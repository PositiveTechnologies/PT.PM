using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements
{
    public class BlockStatement : Statement
    {
        public override NodeType NodeType => NodeType.BlockStatement;

        public List<Statement> Statements { get; set; } = new List<Statement>();

        public BlockStatement(IEnumerable<Statement> statements, FileNode fileNode)
        {
            Statements = statements as List<Statement> ?? statements.ToList();
            if (Statements.Count > 0)
            {
                TextSpan = Statements.First().TextSpan.Union(Statements.Last().TextSpan);
            }
            else
            {
                TextSpan = default(TextSpan);
            }
            FileNode = fileNode;
        }

        public BlockStatement(IEnumerable<Statement> statements, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Statements = statements as List<Statement> ?? statements.ToList();
        }

        public BlockStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            return Statements.Select(s => (UstNode) s).ToArray();
        }

        public override string ToString()
        {
            return "{" + string.Join(Environment.NewLine, Statements) + "}";
        }
    }
}
