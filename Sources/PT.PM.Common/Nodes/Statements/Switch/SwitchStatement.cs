using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchStatement : Statement
    {
        public override NodeType NodeType => NodeType.SwitchStatement;

        public Expression Expression { get; set; }

        public List<SwitchSection> Sections { get; set; }

        public SwitchStatement(Expression expression, IEnumerable<SwitchSection> sections, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Expression = expression;
            Sections = sections as List<SwitchSection> ?? sections.ToList();
        }

        public SwitchStatement()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.Add(Expression);
            result.AddRange(Sections);
            return result.ToArray();
        }
    }
}
