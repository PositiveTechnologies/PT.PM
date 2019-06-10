using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessagePack;

namespace PT.PM.Common.Nodes.Sql
{
    [MessagePackObject]
    public class SqlQuery : Expression
    {
        [Key(0)] public override UstType UstType => UstType.SqlQuery;

        [Key(UstFieldOffset)]
        public Expression QueryCommand { get; set; }

        [Key(UstFieldOffset + 1)]
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
