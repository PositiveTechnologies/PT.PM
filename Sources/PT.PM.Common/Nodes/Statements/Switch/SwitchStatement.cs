using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchStatement : Statement
    {
        public override NodeType NodeType => NodeType.SwitchStatement;

        public Expression Expression { get; set; }

        public IEnumerable<SwitchSection> Sections { get; set; }

        public SwitchStatement(Expression expression, IEnumerable<SwitchSection> sections, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Expression = expression;
            Sections = sections;
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
