using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using MessagePack;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Sql;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.Common.Nodes
{
    [DebuggerDisplay("{" + nameof(ToStringWithoutLineBreaks) + "()}")]

    [MessagePackObject]
    [Union((int)NodeType.RootUst, typeof(RootUst))]

    [Union((int)NodeType.ArgsUst, typeof(ArgsUst))]
    [Union((int)NodeType.Collection, typeof(Collection))]
    [Union((int)NodeType.EntitiesUst, typeof(EntitiesUst))]

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

    [Union((int)NodeType.NamespaceDeclaration, typeof(NamespaceDeclaration))]
    [Union((int)NodeType.TypeDeclaration, typeof(TypeDeclaration))]
    [Union((int)NodeType.UsingDeclaration, typeof(UsingDeclaration))]

    [Union((int)NodeType.ArrayPatternExpression, typeof(ArrayPatternExpression))]
    [Union((int)NodeType.AsExpression, typeof(AsExpression))]
    [Union((int)NodeType.CheckedExpression, typeof(CheckedExpression))]
    [Union((int)NodeType.CommaExpression, typeof(CommaExpression))]
    [Union((int)NodeType.DebuggerStatement, typeof(DebuggerStatement))]
    [Union((int)NodeType.FixedStatement, typeof(FixedStatement))]
    [Union((int)NodeType.LockStatement, typeof(LockStatement))]
    [Union((int)NodeType.UnsafeStatement, typeof(UnsafeStatement))]
    [Union((int)NodeType.WithStatement, typeof(WithStatement))]

    [Union((int)NodeType.QueryArgs, typeof(QueryArgs))]
    [Union((int)NodeType.SqlBlockStatement, typeof(SqlBlockStatement))]
    [Union((int)NodeType.SqlQuery, typeof(SqlQuery))]

    [Union((int)NodeType.SwitchSection, typeof(SwitchSection))]
    [Union((int)NodeType.SwitchStatement, typeof(SwitchStatement))]
    [Union((int)NodeType.CatchClause, typeof(CatchClause))]
    [Union((int)NodeType.TryCatchStatement, typeof(TryCatchStatement))]
    [Union((int)NodeType.BlockStatement, typeof(BlockStatement))]
    [Union((int)NodeType.BreakStatement, typeof(BreakStatement))]
    [Union((int)NodeType.ContinueStatement, typeof(ContinueStatement))]
    [Union((int)NodeType.DoWhileStatement, typeof(DoWhileStatement))]
    [Union((int)NodeType.EmptyStatement, typeof(EmptyStatement))]
    [Union((int)NodeType.ExpressionStatement, typeof(ExpressionStatement))]
    [Union((int)NodeType.ForeachStatement, typeof(ForeachStatement))]
    [Union((int)NodeType.ForStatement, typeof(ForStatement))]
    [Union((int)NodeType.GotoStatement, typeof(GotoStatement))]
    [Union((int)NodeType.IfElseStatement, typeof(IfElseStatement))]
    [Union((int)NodeType.LabelStatement, typeof(LabelStatement))]
    [Union((int)NodeType.ReturnStatement, typeof(ReturnStatement))]
    [Union((int)NodeType.ThrowStatement, typeof(ThrowStatement))]
    [Union((int)NodeType.TypeDeclarationStatement, typeof(TypeDeclarationStatement))]
    [Union((int)NodeType.WhileStatement, typeof(WhileStatement))]
    [Union((int)NodeType.WrapperStatement, typeof(WrapperStatement))]

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

    [Union((int)NodeType.BaseReferenceToken, typeof(BaseReferenceToken))]
    [Union((int)NodeType.IdToken, typeof(IdToken))]
    [Union((int)NodeType.ThisReferenceToken, typeof(ThisReferenceToken))]
    [Union((int)NodeType.TypeToken, typeof(TypeToken))]

    [Union((int)NodeType.ConstructorDeclaration, typeof(ConstructorDeclaration))]
    [Union((int)NodeType.FieldDeclaration, typeof(FieldDeclaration))]
    [Union((int)NodeType.MethodDeclaration, typeof(MethodDeclaration))]
    [Union((int)NodeType.ParameterDeclaration, typeof(ParameterDeclaration))]
    [Union((int)NodeType.PropertyDeclaration, typeof(PropertyDeclaration))]
    [Union((int)NodeType.StatementDeclaration, typeof(StatementDeclaration))]
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>, IUst
    {
        internal const int UstFieldOffset = 2;

        private static readonly PrettyPrinter debuggerPrinter = new PrettyPrinter
        {
            MaxMessageLength = 0,
            ReduceWhitespaces = true
        };

        [IgnoreMember, JsonIgnore]
        public RootUst Root { get; set; }

        [Key(0), JsonProperty("TextSpan"), JsonIgnore] // TODO: back compatibility with external serializers
        public TextSpan[] TextSpans { get; set; } // TODO: make it `protected set`

        [Key(1)]
        public int Key { get; set; }

        [IgnoreMember, JsonIgnore]
        public TextSpan TextSpan
        {
            get => TextSpans?.Length > 0 ? TextSpans[0] : TextSpan.Zero;
            set
            {
                // TODO: try to remove setter (use TextSpans instead)
                if (TextSpans?.Length == 1) // Prevent excess allocation
                {
                    TextSpans[0] = value;
                }
                else
                {
                    TextSpans = new[] {value};
                }
            }
        }

        [IgnoreMember]
        public string Kind => GetType().Name;

        [IgnoreMember]
        public int KindId => GetType().Name.GetHashCode();

        [IgnoreMember]
        public LineColumnTextSpan LineColumnTextSpan => CurrentSourceFile?.GetLineColumnTextSpan(TextSpan);

        [IgnoreMember]
        public TextFile CurrentSourceFile => this is RootUst rootUst ? rootUst.SourceFile : Root?.SourceFile;

        [IgnoreMember]
        public RootUst RootOrThis => this is RootUst rootUst ? rootUst : Root;

        [IgnoreMember]
        public Ust[] Children => GetChildren();

        public int GetKey()
        {
            if (Key != 0)
            {
                return Key;
            }

            return base.GetHashCode();
        }

        public string ToStringWithoutLineBreaks() => debuggerPrinter?.Print(ToString()) ?? "";

        protected Ust()
        {
            TextSpans = new TextSpan[0];
        }

        protected Ust(TextSpan textSpan)
        {
            TextSpans = new [] { textSpan };
        }

        public abstract Ust[] GetChildren();

        public bool Equals(Ust other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(Ust other)
        {
            if (other == null)
            {
                return KindId;
            }

            int nodeTypeCompareResult = KindId - other.KindId;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
        }

        public override string ToString()
        {
            if (Children == null || Children.Length == 0)
            {
                return "";
            }

            var result = new StringBuilder();
            foreach (Ust child in Children)
            {
                result.Append(child);
                result.Append(" ");
            }
            return result.ToString();
        }
    }
}
