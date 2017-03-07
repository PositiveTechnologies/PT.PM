using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements
{
    public class BlockStatement : Statement
    {
        public override NodeType NodeType => NodeType.BlockStatement;

        public IList<Statement> Statements { get; set; } = ArrayUtils<Statement>.EmptyArray;

        public BlockStatement(IList<Statement> statements, FileNode fileNode)
        {
            if (statements.Count > 0)
            {
                TextSpan = statements.First().TextSpan.Union(statements.Last().TextSpan);
            }
            else
            {
                TextSpan = default(TextSpan);
            }
            Statements = statements;
            FileNode = fileNode;
        }

        public BlockStatement(IList<Statement> statements, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Statements = statements;
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
