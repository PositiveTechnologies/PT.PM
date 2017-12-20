using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Tokens
{
    public abstract class Token : Expression
    {
        public abstract string TextValue { get; }

        public override bool IsTerminal => true;

        public Expression Expression { get; set; }

        protected Token(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Token()
        {
        }

        public sealed override Ust[] GetChildren()
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
                return 1;
            }

            if (!other.IsTerminal)
            {
                return -1;
            }

            var nodeTypeResult = KindId - other.KindId;
            if (nodeTypeResult != 0)
            {
                return nodeTypeResult;
            }

            Token otherLiteral = (Token)other;
            if (Expression != null && otherLiteral.Expression == null)
            {
                return 1;
            }

            if (Expression == null && otherLiteral.Expression != null)
            {
                return -1;
            }

            if (Expression != null && otherLiteral.Expression != null)
            {
                var expressionCompareResult = Expression.CompareTo(otherLiteral.Expression);
                if (expressionCompareResult != 0)
                    return expressionCompareResult;
            }

            return 0;
        }

        public override string ToString()
        {
            return TextValue;
        }
    }
}
