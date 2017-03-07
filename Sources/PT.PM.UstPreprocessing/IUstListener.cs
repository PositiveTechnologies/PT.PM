using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Dsl;
using PT.PM.Patterns.Nodes;

namespace PT.PM.UstPreprocessing
{
    public interface IUstListener
    {
        void Walk(Node node);

        #region Collections

        void Enter(ArgsNode argsNode);

        void Enter(EntitiesNode entitiesNode);

        void Exit(ArgsNode argsNode);

        void Exit(EntitiesNode entitiesNode);

        #endregion

        #region Expression

        void Enter(AnonymousMethodExpression anonymousMethodExpression);
        void Enter(ArrayCreationExpression arrayCreationExpression);
        void Enter(AssignmentExpression assignmentExpression);
        void Enter(BaseReferenceExpression baseReferenceExpression);
        void Enter(BinaryOperatorExpression binaryOperatorExpression);
        void Enter(CastExpression castExpression);
        void Enter(ConditionalExpression conditionalExpression);
        void Enter(WrapperExpression wrapperExpression);
        void Enter(IndexerExpression indexerExpression);
        void Enter(InvocationExpression invocationExpression);
        void Enter(MemberReferenceExpression memberReferenceExpression);
        void Enter(MultichildExpression multichildExpression);
        void Enter(ObjectCreateExpression objectCreateExpression);
        void Enter(UnaryOperatorExpression unaryOperatorExpression);
        void Enter(VariableDeclarationExpression variableDeclarationExpression);

        void Exit(AnonymousMethodExpression anonymousMethodExpression);
        void Exit(ArrayCreationExpression arrayCreationExpression);
        void Exit(AssignmentExpression assignmentExpression);
        void Exit(BaseReferenceExpression baseReferenceExpression);
        void Exit(BinaryOperatorExpression binaryOperatorExpression);
        void Exit(CastExpression castExpression);
        void Exit(ConditionalExpression conditionalExpression);
        void Exit(WrapperExpression wrapperExpression);
        void Exit(IndexerExpression indexerExpression);
        void Exit(InvocationExpression invocationExpression);
        void Exit(MemberReferenceExpression memberReferenceExpression);
        void Exit(MultichildExpression multichildExpression);
        void Exit(ObjectCreateExpression objectCreateExpression);
        void Exit(UnaryOperatorExpression unaryOperatorExpression);
        void Exit(VariableDeclarationExpression variableDeclarationExpression);

        #endregion

        #region GeneralScope

        void Enter(NamespaceDeclaration namespaceDeclaration);
        void Enter(TypeDeclaration typeDeclaration);
        void Enter(UsingDeclaration usingDeclaration);

        void Exit(NamespaceDeclaration namespaceDeclaration);
        void Exit(TypeDeclaration typeDeclaration);
        void Exit(UsingDeclaration usingDeclaration);

        #endregion

        #region Literals

        void Enter(BinaryOperatorLiteral binaryOperatorLiteral);
        void Enter(BooleanLiteral booleanLiteral);
        void Enter(CommentLiteral commentLiteral);
        void Enter(FloatLiteral floatLiteral);
        void Enter(IdToken idToken);
        void Enter(IntLiteral intLiteral);
        void Enter(ModifierLiteral modifierLiteral);
        void Enter(NullLiteral nullLiteral);
        void Enter(ParameterModifierLiteral parameterModifierLiteral);
        void Enter(StringLiteral stringLiteral);
        void Enter(ThisReferenceToken thisReferenceLiteral);
        void Enter(TypeToken typeToken);
        void Enter(TypeTypeLiteral typeTypeLiteral);
        void Enter(UnaryOperatorLiteral unaryOperatorLiteral);

        void Exit(BinaryOperatorLiteral binaryOperatorLiteral);
        void Exit(BooleanLiteral booleanLiteral);
        void Exit(CommentLiteral commentLiteral);
        void Exit(FloatLiteral floatLiteral);
        void Exit(IdToken idToken);
        void Exit(IntLiteral intLiteral);
        void Exit(ModifierLiteral modifierLiteral);
        void Exit(NullLiteral nullLiteral);
        void Exit(ParameterModifierLiteral parameterModifierLiteral);
        void Exit(StringLiteral stringLiteral);
        void Exit(ThisReferenceToken thisReferenceLiteral);
        void Exit(TypeToken typeToken);
        void Exit(TypeTypeLiteral typeTypeLiteral);
        void Exit(UnaryOperatorLiteral unaryOperatorLiteral);

        #endregion

        #region Specific

        void Enter(AsExpression asExpression);
        void Enter(CheckedExpression checkedExpression);
        void Enter(CheckedStatement checkedStatement);
        void Enter(CSharpParameterDeclaration cSharpParameterDeclaration);
        void Enter(FixedStatement fixedStatement);
        void Enter(LockStatement lockStatement);
        void Enter(UnsafeStatement unsafeStatement);

        void Exit(AsExpression asExpression);
        void Exit(CheckedExpression checkedExpression);
        void Exit(CheckedStatement checkedStatement);
        void Exit(CSharpParameterDeclaration cSharpParameterDeclaration);
        void Exit(FixedStatement fixedStatement);
        void Exit(LockStatement lockStatement);
        void Exit(UnsafeStatement unsafeStatement);

        #endregion

        #region Statements

        void Enter(SwitchSection switchSection);
        void Enter(SwitchStatement switchStatement);
        void Enter(CatchClause catchClause);
        void Enter(TryCatchStatement tryCatchStatement);
        void Enter(BlockStatement blockStatement);
        void Enter(BreakStatement breakStatement);
        void Enter(ContinueStatement continueStatement);
        void Enter(DoWhileStatement doWhileStatement);
        void Enter(EmptyStatement emptyStatement);
        void Enter(ExpressionStatement expressionStatement);
        void Enter(ForeachStatement foreachStatement);
        void Enter(ForStatement forStatement);
        void Enter(GotoStatement gotoStatement);
        void Enter(IfElseStatement ifElseStatement);
        void Enter(ReturnStatement returnStatement);
        void Enter(ThrowStatement throwStatement);
        void Enter(TypeDeclarationStatement typeDeclarationStatement);
        void Enter(WhileStatement whileStatement);
        void Enter(WithStatement withStatement);
        void Enter(WrapperStatement wrapperStatement);

        void Exit(SwitchSection switchSection);
        void Exit(SwitchStatement switchStatement);
        void Exit(CatchClause catchClause);
        void Exit(TryCatchStatement tryCatchStatement);
        void Exit(BlockStatement blockStatement);
        void Exit(BreakStatement breakStatement);
        void Exit(ContinueStatement continueStatement);
        void Exit(DoWhileStatement doWhileStatement);
        void Exit(EmptyStatement emptyStatement);
        void Exit(ExpressionStatement expressionStatement);
        void Exit(ForeachStatement foreachStatement);
        void Exit(ForStatement forStatement);
        void Exit(GotoStatement gotoStatement);
        void Exit(IfElseStatement ifElseStatement);
        void Exit(ReturnStatement returnStatement);
        void Exit(ThrowStatement throwStatement);
        void Exit(TypeDeclarationStatement typeDeclarationStatement);
        void Exit(WhileStatement whileStatement);
        void Exit(WithStatement withStatement);
        void Exit(WrapperStatement wrapperStatement);

        #endregion

        #region TypeMembers

        void Enter(ConstructorDeclaration constructorDeclaration);
        void Enter(FieldDeclaration fieldDeclaration);
        void Enter(MethodDeclaration methodDeclaration);
        void Enter(ParameterDeclaration parameterDeclaration);
        void Enter(StatementDeclaration statementDeclaration);

        void Exit(ConstructorDeclaration constructorDeclaration);
        void Exit(FieldDeclaration fieldDeclaration);
        void Exit(MethodDeclaration methodDeclaration);
        void Exit(ParameterDeclaration parameterDeclaration);
        void Exit(StatementDeclaration statementDeclaration);

        #endregion

        #region Other

        void Enter(FileNode fileNode);
        void Enter(NotImplementedNode notImplementedNode);

        void Exit(FileNode fileNode);
        void Exit(NotImplementedNode notImplementedNode);

        #endregion

        #region Patterns

        void Enter(PatternNode patternVars);
        void Enter(PatternBooleanLiteral patternBooleanLiteral);
        void Enter(PatternComment patternComment);
        void Enter(PatternExpression patternExpression);
        void Enter(PatternExpressionInsideExpression patternExpressionInsideExpression);
        void Enter(PatternExpressionInsideStatement patternExpressionInsideStatement);
        void Enter(PatternExpressions patternExpressions);
        void Enter(PatternIdToken patternIdToken);
        void Enter(PatternIfElseStatement patternIfElseStatement);
        void Enter(PatternIntLiteral patternIntLiteral);
        void Enter(PatternMultipleExpressions patternMultiExpressions);
        void Enter(PatternMultipleStatements patternMultiStatements);
        void Enter(PatternStatement patternStatement);
        void Enter(PatternStatements patternStatements);
        void Enter(PatternStringLiteral patternStringLiteral);
        void Enter(PatternTryCatchStatement patternTryCatchStatement);
        void Enter(PatternVarDef patternVarDef);
        void Enter(PatternVarRef patternVarRef);
        void Exit(PatternNode patternVars);
        void Exit(PatternBooleanLiteral patternBooleanLiteral);
        void Exit(PatternComment patternComment);
        void Exit(PatternExpression patternExpression);
        void Exit(PatternExpressionInsideExpression patternExpressionInsideExpression);
        void Exit(PatternExpressionInsideStatement patternExpressionInsideStatement);
        void Exit(PatternExpressions patternExpressions);
        void Exit(PatternIdToken patternIdToken);
        void Exit(PatternIfElseStatement patternIfElseStatement);
        void Exit(PatternIntLiteral patternIntLiteral);
        void Exit(PatternMultipleExpressions patternMultiExpressions);
        void Exit(PatternMultipleStatements patternMultiStatements);
        void Exit(PatternStatement patternStatement);
        void Exit(PatternStatements patternStatements);
        void Exit(PatternStringLiteral patternStringLiteral);
        void Exit(PatternTryCatchStatement patternTryCatchStatement);
        void Exit(PatternVarDef patternVarDef);
        void Exit(PatternVarRef patternVarRef);

        #endregion

        #region Dsl

        void Enter(DslNode patternExpression);
        void Enter(LangCodeNode langCodeNode);

        void Exit(DslNode patternExpression);
        void Exit(LangCodeNode langCodeNode);

        #endregion
    }
}
