using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;
using MessagePack;

namespace PT.PM.Common.Nodes.Sql
{
    [MessagePackObject]
    public class QueryArgs : Expression
    {
        [Key(0)] public override UstType UstType => UstType.QueryArgs;

        [Key(UstFieldOffset)]
        public Collection Parameters { get; set; }

        public QueryArgs(IEnumerable<Expression> elements, TextSpan textSpan)
            : base(textSpan)
        {
            Parameters = new Collection(elements ?? throw new ArgumentNullException(nameof(elements)));
        }

        public QueryArgs()
        {
        }

        public override Expression[] GetArgs() => new Expression[] { this };

        public override Ust[] GetChildren() => Parameters.Collection.ToArray();

        public override string ToString() => string.Join(", ", Parameters.Collection);
    }
}
