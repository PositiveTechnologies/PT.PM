using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class MultichildExpression : Expression
    {
        [Key(0)] public override UstType UstType => UstType.MultichildExpression;

        [Key(UstFieldOffset)]
        public List<Expression> Expressions { get; set; }

        public MultichildExpression(IEnumerable<Expression> children, TextSpan textSpan)
            : base(textSpan)
        {
            Expressions = children as List<Expression> ?? children.ToList();
        }

        public MultichildExpression(IEnumerable<Expression> children)
        {
            Expressions = children as List<Expression> ?? children.ToList();
            TextSpan = Expressions.Count > 0 ? Expressions.First().TextSpan.Union(Expressions.Last().TextSpan) : TextSpan.Zero;
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

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>(Expressions);
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
