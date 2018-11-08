using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PT.PM.Common.Nodes.Sql
{
    public class SqlQuery : Expression
    {
        public Expression QueryCommand { get; set; }

        public List<Expression> QueryElements { get; set; } = new List<Expression>();

        public SqlQuery(Expression queryKey, IEnumerable<Expression> queryElements,TextSpan textSpan)
            : base(textSpan)
        {
            QueryCommand = queryKey ?? throw new ArgumentNullException(nameof(queryKey));
            QueryElements = queryElements?.ToList() ?? QueryElements;
        }

        public SqlQuery()
        {
        }

        public override Ust[] GetChildren() => GetArgs();

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

        public override Expression[] GetArgs()
        {
            var args = new List<Expression>() { QueryCommand };
            args.AddRange(QueryElements);
            return args.ToArray();
        }
    }
}
