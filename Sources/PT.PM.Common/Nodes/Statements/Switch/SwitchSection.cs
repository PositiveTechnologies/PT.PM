using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchSection : UstNode
    {
        public override NodeType NodeType => NodeType.SwitchSection;

        public IEnumerable<Expression> CaseLabels { get; set; }

        public IEnumerable<Statement> Statements { get; set; }

        public SwitchSection(IEnumerable<Expression> caseLabels, IEnumerable<Statement> statements, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            CaseLabels = caseLabels;
            Statements = statements;
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
