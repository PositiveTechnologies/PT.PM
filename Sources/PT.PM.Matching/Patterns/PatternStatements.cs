using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternStatements : PatternBase
    {
        public List<PatternBase> Statements { get; set; } = new List<PatternBase>();

        public PatternStatements()
        {
        }

        public PatternStatements(IEnumerable<PatternBase> statements)
        {
            Statements = statements?.ToList() ?? new List<PatternBase>();
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => string.Join(";\n", Statements);

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.BlockStatement)
            {
                return false;
            }

            if (Statements == null)
            {
                return true;
            }

            IList<Statement> otherStatements = ((Common.Nodes.Statements.BlockStatement)ust).Statements;

            throw new NotImplementedException();
        }
    }
}
