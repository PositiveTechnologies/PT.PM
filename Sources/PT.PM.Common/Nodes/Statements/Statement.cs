using MessagePack;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Sql;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;

namespace PT.PM.Common.Nodes.Statements
{
    [MessagePackObject]
    [Union((int)NodeType.SwitchStatement, typeof(SwitchStatement))]
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
    [Union((int)NodeType.SqlBlockStatement, typeof(SqlBlockStatement))]
    [Union((int)NodeType.DebuggerStatement, typeof(DebuggerStatement))]
    [Union((int)NodeType.FixedStatement, typeof(FixedStatement))]
    [Union((int)NodeType.LockStatement, typeof(LockStatement))]
    [Union((int)NodeType.UnsafeStatement, typeof(UnsafeStatement))]
    [Union((int)NodeType.WithStatement, typeof(WithStatement))]
    public abstract class Statement : Ust
    {
        protected Statement(TextSpan textSpan)
            : base(textSpan)
        {
        }

        protected Statement()
        {
        }
    }
}
