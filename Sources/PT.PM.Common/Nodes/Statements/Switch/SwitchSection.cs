using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchSection : UstNode
    {
        public override NodeType NodeType => NodeType.SwitchSection;

        public List<Expression> CaseLabels { get; set; }

        public List<Statement> Statements { get; set; }

        public SwitchSection(IEnumerable<Expression> caseLabels, IEnumerable<Statement> statements, TextSpan textSpan)
            : base(textSpan)
        {
            CaseLabels = caseLabels as List<Expression> ?? caseLabels.ToList();
            Statements = statements as List<Statement> ?? statements.ToList();
        }

        public SwitchSection()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(CaseLabels);
            result.AddRange(Statements);
            return result.ToArray();
        }
    }
}
