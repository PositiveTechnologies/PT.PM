using System;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMultipleExpressions : Expression
    {
        public override UstKind Kind => UstKind.PatternMultipleExpressions;

        public PatternMultipleExpressions()
        {
        }

        public override Ust[] GetChildren()
        {
            return ArrayUtils<Ust>.EmptyArray;
        }

        public override Expression[] GetArgs()
        {
            return new Expression[] { this };
        }

        public override int CompareTo(Ust other)
        {
            if (other == null)
            {
                return (int)other.Kind;
            }

            if (other is Expression)
            {
                return 0;
            }
            else
            {
                return Kind - other.Kind;
            }
        }

        public override string ToString() => "#*";
    }
}
