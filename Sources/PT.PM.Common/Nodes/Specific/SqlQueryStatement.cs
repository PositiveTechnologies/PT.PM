using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PT.PM.Common.Nodes.Specific
{
    public class SqlQueryStatement : Statement
    {
        public Expression QueryCommand { get; set; }

        public List<Expression> QueryElements { get; set; } = new List<Expression>();

        public SqlQueryStatement(Expression queryKey, IEnumerable<Expression> queryElements,TextSpan textSpan)
            : base(textSpan)
        {
            QueryCommand = queryKey ?? throw new ArgumentNullException(nameof(queryKey));
            QueryElements = queryElements?.ToList() ?? QueryElements;
        }

        public SqlQueryStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var children = new List<Ust>() { QueryCommand };
            children.AddRange(QueryElements);
            return children.ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(QueryCommand);
            builder.Append(" ");
            foreach (var expr in QueryElements)
            {
                builder.Append(expr);
                builder.Append(" ");
            }
            return builder.ToString();
        }
    }
}
