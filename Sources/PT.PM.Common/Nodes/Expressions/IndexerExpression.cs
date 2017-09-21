using PT.PM.Common.Nodes.Collections;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class IndexerExpression : Expression
    {
        public override UstKind Kind => UstKind.IndexerExpression;

        public Expression Target { get; set; }

        public ArgsUst Arguments { get; set; }

        public IndexerExpression(Expression target, ArgsUst args, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Arguments = args;
        }

        public IndexerExpression()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] { Target, Arguments };
        }

        public override Expression[] GetArgs()
        {
            var result = new List<Expression> { Target };
            result.AddRange(Arguments.Collection);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{Target}[{string.Join(",", Arguments)}]";
        }
    }
}
