using MessagePack;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public class BaseReferenceToken : Expression
    {
        [Key(0)] public override UstType UstType => UstType.BaseReferenceToken;

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
