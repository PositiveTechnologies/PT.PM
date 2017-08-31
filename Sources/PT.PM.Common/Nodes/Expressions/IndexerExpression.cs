using PT.PM.Common.Nodes.Collections;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class IndexerExpression : Expression
    {
        public override NodeType NodeType => NodeType.IndexerExpression;

        public Expression Target { get; set; }

        public ArgsNode Arguments { get; set; }

        public IndexerExpression(Expression target, ArgsNode args, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Arguments = args;
        }

        public IndexerExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new UstNode[] { Target, Arguments };
            return result;
        }

        public override string ToString()
        {
            return $"{Target}[{string.Join(",", Arguments)}]";
        }
    }
}
