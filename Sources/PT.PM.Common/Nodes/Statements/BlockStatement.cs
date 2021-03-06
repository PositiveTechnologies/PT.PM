﻿using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    public class BlockStatement : Statement
    {
        [Key(0)] public override UstType UstType => UstType.BlockStatement;

        [Key(UstFieldOffset)]
        public List<Statement> Statements { get; set; } = new List<Statement>();

        public BlockStatement(IEnumerable<Statement> statements)
        {
            Statements = statements as List<Statement> ?? statements.ToList();
            TextSpan = Statements.GetTextSpan();
        }

        public BlockStatement(IEnumerable<Statement> statements, TextSpan textSpan)
            : base(textSpan)
        {
            Statements = statements as List<Statement> ?? statements.ToList();
        }

        public BlockStatement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public BlockStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return Statements.Select(s => (Ust)s).ToArray();
        }

        public override string ToString()
        {

            return "{\n" + string.Join("\n", Statements).ToStringWithTrailNL() + "}";
        }
    }
}
