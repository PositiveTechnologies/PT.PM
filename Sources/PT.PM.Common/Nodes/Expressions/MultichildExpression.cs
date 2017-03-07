using System.Linq;
using System.Collections.Generic;

namespace PT.PM.Common.Nodes.Expressions
{
    public class MultichildExpression : Expression
    {
        public override NodeType NodeType => NodeType.MultichildExpression;

        public IList<Expression> Expressions { get; set; }

        public MultichildExpression(IList<Expression> children, TextSpan textSpan, FileNode fileNode)
            : base(textSpan, fileNode)
        {
            Expressions = children;
        }

        public MultichildExpression(IList<Expression> children, FileNode fileNode)
        {
            if (children.Count > 0)
            {
                TextSpan = children.First().TextSpan.Union(children.Last().TextSpan);
            }
            else
            {
                TextSpan = default(TextSpan);
            }
            Expressions = children;
        }

        public MultichildExpression(TextSpan textSpan, FileNode fileNode, params Expression[] children)
            : base(textSpan, fileNode)
        {
            Expressions = children;
        }

        public MultichildExpression()
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>(Children);
            return result.ToArray();
        }

        public override string ToString()
        {
            return $"MutliExpr({string.Join(", ", Expressions)}";
        }
    }
}
