using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchSection : Ust
    {
        public List<Expression> CaseLabels { get; set; } = new List<Expression>();

        public List<Statement> Statements { get; set; } = new List<Statement>();

        public SwitchSection(IEnumerable<Expression> caseLabels, IEnumerable<Statement> statements, TextSpan textSpan)
            : base(textSpan)
        {
            CaseLabels = caseLabels as List<Expression> ?? caseLabels.ToList();
            Statements = statements as List<Statement> ?? statements.ToList();
        }

        public SwitchSection()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(CaseLabels);
            result.AddRange(Statements);
            return result.ToArray();
        }
    }
}
