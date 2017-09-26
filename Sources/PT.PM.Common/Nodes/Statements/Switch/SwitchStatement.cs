using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;

namespace PT.PM.Common.Nodes.Statements.Switch
{
    public class SwitchStatement : Statement
    {
        public Expression Expression { get; set; }

        public List<SwitchSection> Sections { get; set; }

        public SwitchStatement(Expression expression, IEnumerable<SwitchSection> sections, TextSpan textSpan)
            : base(textSpan)
        {
            Expression = expression;
            Sections = sections as List<SwitchSection> ?? sections.ToList();
        }

        public SwitchStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Expression);
            result.AddRange(Sections);
            return result.ToArray();
        }
    }
}
