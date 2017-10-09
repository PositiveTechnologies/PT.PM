using System.Collections.Generic;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Expressions;
using System.Linq;

namespace PT.PM.Common.Nodes.Specific
{
    public class FixedStatement : SpecificStatement
    {
        public TypeToken Type { get; set; }

        public List<AssignmentExpression> Variables { get; set; }

        public Statement Embedded { get; set; }

        public FixedStatement(TypeToken type, IEnumerable<AssignmentExpression> vars, Statement embedded, TextSpan textSpan)
            : base(textSpan)
        {
            Type = type;
            Variables = vars as List<AssignmentExpression> ?? vars.ToList();
            Embedded = embedded;
        }

        public FixedStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Variables);
            result.Add(Embedded);
            return result.ToArray();
        }
    }
}
