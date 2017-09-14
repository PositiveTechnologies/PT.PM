using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class MultichildExpression : Expression
    {
        public override NodeType NodeType => NodeType.MultichildExpression;

        public List<Expression> Expressions { get; set; }

        public MultichildExpression(IEnumerable<Expression> children, TextSpan textSpan)
            : base(textSpan)
        {
            Expressions = children as List<Expression> ?? children.ToList();
        }

        public MultichildExpression(IEnumerable<Expression> children)
        {
            Expressions = children as List<Expression> ?? children.ToList();
            if (Expressions.Count > 0)
            {
                TextSpan = Expressions.First().TextSpan.Union(Expressions.Last().TextSpan);
            }
            else
            {
                TextSpan = default(TextSpan);
            }
        }

        public MultichildExpression(TextSpan textSpan, RootNode fileNode, params Expression[] children)
            : base(textSpan)
        {
            Expressions = children.ToList();
        }

        public MultichildExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(Expressions);
            return result.ToArray();
        }

        public override Expression[] GetArgs()
        {
            return Expressions.ToArray();
        }

        public override string ToString()
        {
            return $"MutliExpr({string.Join(", ", Expressions)}";
        }
    }
}
