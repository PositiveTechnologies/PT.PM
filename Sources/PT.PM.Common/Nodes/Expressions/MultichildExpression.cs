using System.Linq;
using System.Collections.Generic;
using System;

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

        /// MultichildExpression can express an array initializer.
        /// This function does some work to get dimensions of the array from
        /// view of its initializer. For example { { 1, 2} , { 1, 2} } init
        /// 2d array. multichildExpressionArgNumber is the expected position number of the
        /// child which type should be MultichildExpression
        public int GetDepth(int multichildExpressionArgNumber)
        {
            var count = Expressions.Count();
            if (count > 2 && multichildExpressionArgNumber >= count)
            {
                throw new ArgumentException("multichildExpressionArgNumber should be " +
                    "less than the number of Expressions elements");
            }
            return 1 +
                (count > 2 && Expressions[multichildExpressionArgNumber] is MultichildExpression child ?
                    child.GetDepth(multichildExpressionArgNumber) : 0);
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
