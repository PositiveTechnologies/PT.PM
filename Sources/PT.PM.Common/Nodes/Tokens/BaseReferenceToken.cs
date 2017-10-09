using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Tokens
{
    public class BaseReferenceToken : Expression
    {
        public BaseReferenceToken(TextSpan textSpan)
            : base(textSpan)
        {
        }

        public BaseReferenceToken()
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

        public override string ToString()
        {
            return "base";
        }
    }
}
