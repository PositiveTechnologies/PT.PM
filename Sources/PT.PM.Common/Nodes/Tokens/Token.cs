using MessagePack;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Tokens
{
    [MessagePackObject]
    
    [Union((int)NodeType.IdToken, typeof(IdToken))]
    [Union((int)NodeType.ThisReferenceToken, typeof(ThisReferenceToken))]
    [Union((int)NodeType.TypeToken, typeof(TypeToken))]
    
    [Union((int)NodeType.BinaryOperatorLiteral, typeof(BinaryOperatorLiteral))]
    [Union((int)NodeType.BooleanLiteral, typeof(BooleanLiteral))]
    [Union((int)NodeType.CommentLiteral, typeof(CommentLiteral))]
    [Union((int)NodeType.FloatLiteral, typeof(FloatLiteral))]
    [Union((int)NodeType.InOutModifierLiteral, typeof(InOutModifierLiteral))]
    [Union((int)NodeType.IntLiteral, typeof(IntLiteral))]
    [Union((int)NodeType.ModifierLiteral, typeof(ModifierLiteral))]
    [Union((int)NodeType.NullLiteral, typeof(NullLiteral))]
    [Union((int)NodeType.StringLiteral, typeof(StringLiteral))]
    [Union((int)NodeType.TypeTypeLiteral, typeof(TypeTypeLiteral))]
    [Union((int)NodeType.UnaryOperatorLiteral, typeof(UnaryOperatorLiteral))]
    public abstract class Token : Expression
    {
        [IgnoreMember]
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
