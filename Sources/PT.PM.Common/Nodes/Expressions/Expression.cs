using MessagePack;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Sql;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]

    [Union((int)NodeType.AnonymousMethodExpression, typeof(AnonymousMethodExpression))]
    [Union((int)NodeType.AnonymousObjectExpression, typeof(AnonymousObjectExpression))]
    [Union((int)NodeType.ArgumentExpression, typeof(ArgumentExpression))]
    [Union((int)NodeType.ArrayCreationExpression, typeof(ArrayCreationExpression))]
    [Union((int)NodeType.AssignmentExpression, typeof(AssignmentExpression))]
    [Union((int)NodeType.BinaryOperatorExpression, typeof(BinaryOperatorExpression))]
    [Union((int)NodeType.CastExpression, typeof(CastExpression))]
    [Union((int)NodeType.ConditionalExpression, typeof(ConditionalExpression))]
    [Union((int)NodeType.IndexerExpression, typeof(IndexerExpression))]
    [Union((int)NodeType.InvocationExpression, typeof(InvocationExpression))]
    [Union((int)NodeType.MemberReferenceExpression, typeof(MemberReferenceExpression))]
    [Union((int)NodeType.MultichildExpression, typeof(MultichildExpression))]
    [Union((int)NodeType.ObjectCreateExpression, typeof(ObjectCreateExpression))]
    [Union((int)NodeType.TupleCreateExpression, typeof(TupleCreateExpression))]
    [Union((int)NodeType.UnaryOperatorExpression, typeof(UnaryOperatorExpression))]
    [Union((int)NodeType.VariableDeclarationExpression, typeof(VariableDeclarationExpression))]
    [Union((int)NodeType.WrapperExpression, typeof(WrapperExpression))]
    [Union((int)NodeType.YieldExpression, typeof(YieldExpression))]

    [Union((int)NodeType.BaseReferenceToken, typeof(BaseReferenceToken))]
    [Union((int)NodeType.IdToken, typeof(IdToken))]
    [Union((int)NodeType.ThisReferenceToken, typeof(ThisReferenceToken))]
    [Union((int)NodeType.TypeToken, typeof(TypeToken))]
    [Union((int)NodeType.QueryArgs, typeof(QueryArgs))]
    [Union((int)NodeType.SqlQuery, typeof(SqlQuery))]
    [Union((int)NodeType.ArrayPatternExpression, typeof(ArrayPatternExpression))]
    [Union((int)NodeType.AsExpression, typeof(AsExpression))]
    [Union((int)NodeType.CheckedExpression, typeof(CheckedExpression))]
    [Union((int)NodeType.CommaExpression, typeof(CommaExpression))]

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
    public abstract class Expression : Ust
    {
        protected Expression(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Expression()
        {
        }

        public abstract Expression[] GetArgs();
    }
}
