using System.Linq;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class MultichildExpression : Expression
    {
        public override NodeType NodeType => NodeType.MultichildExpression;

        public List<Expression> Expressions { get; set; }

        public MultichildExpression(IEnumerable<Expression> children, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Expressions = children as List<Expression> ?? children.ToList();
        }

        public MultichildExpression(IEnumerable<Expression> children, FileNode fileNode)
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

        public MultichildExpression(TextSpan textSpan, FileNode fileNode, params Expression[] children)
            : base(textSpan, fileNode)
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

        public override string ToString()
        {
            return $"MutliExpr({string.Join(", ", Expressions)}";
        }
    }
}
