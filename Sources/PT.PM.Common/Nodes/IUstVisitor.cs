﻿using PT.PM.Common.Nodes.Collections;
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
    public interface IUstVisitor<out T>
    {
        #region Abstract

        T Visit(Ust ust);

        #endregion

        #region Collections

        T Visit(ArgsUst argsUst);
        T Visit(EntitiesUst entitiesUst);

        #endregion

        #region Expressions

        T Visit(AnonymousMethodExpression anonymousMethodExpression);
        T Visit(AnonymousObjectExpression anonymousObjectExpression);
        T Visit(ArrayCreationExpression arrayCreationExpression);
        T Visit(AssignmentExpression assignmentExpression);
        T Visit(BaseReferenceToken baseReferenceExpression);
        T Visit(BinaryOperatorExpression binaryOperatorExpression);
        T Visit(CastExpression castExpression);
        T Visit(ConditionalExpression conditionalExpression);
        T Visit(WrapperExpression wrapperExpression);
        T Visit(IndexerExpression indexerExpression);
        T Visit(InvocationExpression invocationExpression);
        T Visit(MemberReferenceExpression memberReferenceExpression);
        T Visit(MultichildExpression multichildExpression);
        T Visit(ObjectCreateExpression objectCreateExpression);
        T Visit(TupleCreateExpression tupleCreateExpression);
        T Visit(UnaryOperatorExpression unaryOperatorExpression);
        T Visit(VariableDeclarationExpression variableDeclarationExpression);
        T Visit(CommaExpression colonExpression);
        T Visit(ArgumentExpression argumentExpression);
        T Visit(YieldExpression yieldExpression);

        #endregion

        #region GeneralScope

        T Visit(NamespaceDeclaration namespaceDeclaration);
        T Visit(TypeDeclaration typeDeclaration);
        T Visit(UsingDeclaration usingDeclaration);

        #endregion

        #region Literals

        T Visit(BinaryOperatorLiteral binaryOperatorLiteral);
        T Visit(BooleanLiteral booleanLiteral);
        T Visit(Comment comment);
        T Visit(FloatLiteral floatLiteral);
        T Visit(IdToken idToken);
        T Visit(IntLiteral intLiteral);
        T Visit(BigIntLiteral bigIntLiteral);
        T Visit(LongLiteral longLiteral);
        T Visit(ModifierLiteral modifierLiteral);
        T Visit(NullLiteral nullLiteral);
        T Visit(InOutModifierLiteral parameterModifierLiteral);
        T Visit(StringLiteral stringLiteral);
        T Visit(ThisReferenceToken thisReferenceLiteral);
        T Visit(TypeToken typeToken);
        T Visit(TypeTypeLiteral typeTypeLiteral);
        T Visit(UnaryOperatorLiteral unaryOperatorLiteral);

        #endregion

        #region Specific

        T Visit(AsExpression asExpression);
        T Visit(Attribute attribute);
        T Visit(CheckedExpression checkedExpression);
        T Visit(FixedStatement fixedStatement);
        T Visit(LockStatement lockStatement);
        T Visit(UnsafeStatement unsafeStatement);
        T Visit(ArrayPatternExpression arrayPatternExpression);
        T Visit(QueryArgs queryParameters);
        T Visit(SqlQuery sqlQueryStatement);
        T Visit(SqlBlockStatement sqlBlockStatement);

        #endregion

        #region Statements

        T Visit(SwitchSection switchSection);
        T Visit(SwitchStatement switchStatement);
        T Visit(CatchClause catchClause);
        T Visit(TryCatchStatement tryCatchStatement);
        T Visit(BlockStatement blockStatement);
        T Visit(BreakStatement breakStatement);
        T Visit(ContinueStatement continueStatement);
        T Visit(DebuggerStatement debuggerStatement);
        T Visit(DoWhileStatement doWhileStatement);
        T Visit(EmptyStatement emptyStatement);
        T Visit(ExpressionStatement expressionStatement);
        T Visit(ForeachStatement foreachStatement);
        T Visit(ForStatement forStatement);
        T Visit(GotoStatement gotoStatement);
        T Visit(IfElseStatement ifElseStatement);
        T Visit(LabelStatement labelStatement);
        T Visit(ReturnStatement returnStatement);
        T Visit(ThrowStatement throwStatement);
        T Visit(TypeDeclarationStatement typeDeclarationStatement);
        T Visit(WhileStatement whileStatement);
        T Visit(WithStatement withStatement);
        T Visit(WrapperStatement wrapperStatement);

        #endregion

        #region TypeMembers

        T Visit(ConstructorDeclaration constructorDeclaration);
        T Visit(FieldDeclaration fieldDeclaration);
        T Visit(PropertyDeclaration propertyDeclaration);
        T Visit(MethodDeclaration methodDeclaration);
        T Visit(ParameterDeclaration parameterDeclaration);
        T Visit(StatementDeclaration statementDeclaration);

        #endregion

        #region Other

        T Visit(RootUst rootUst);
        T Visit(Collection collection);
        T Visit(Keyword keyword);
        T Visit(Operator op);
        T Visit(Punctuator punctuator);
        T Visit(RawToken rawToken);

        #endregion
    }
}
