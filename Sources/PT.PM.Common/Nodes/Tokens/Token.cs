using MessagePack;
using Newtonsoft.Json;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    public abstract class Token : Expression, ITerminal
    {
        [IgnoreMember, JsonIgnore]
        public abstract string TextValue { get; }

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

            var nodeTypeResult = KindId - other.KindId;
            if (nodeTypeResult != 0)
            {
                return nodeTypeResult;
            }

            return 0;
        }

        public override string ToString() => TextValue;
    }
}
