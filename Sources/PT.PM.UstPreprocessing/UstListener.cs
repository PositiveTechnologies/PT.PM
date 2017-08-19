using System;
using System.Linq;
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
using System.Reflection;
using PT.PM.Common;
using System.Collections;
using PT.PM.Common.Symbols;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.UstPreprocessing
{
    public class UstListener : IUstListener
    {
        public void Walk(Node node)
        {
            dynamic dynamicNode = node;
            Enter(dynamicNode);
            Visit(node);
            Exit(dynamicNode);
        }

        #region Collections

        public virtual void Enter(ArgsNode argsNode)
        {
        }

        public virtual void Enter(EntitiesNode entitiesNode)
        {
        }

        public virtual void Exit(ArgsNode argsNode)
        {
        }

        public virtual void Exit(EntitiesNode entitiesNode)
        {
        }

        #endregion

        #region Expression

        public virtual void Enter(AnonymousMethodExpression anonymousMethodExpression)
        {
        }
        public virtual void Enter(ArrayCreationExpression arrayCreationExpression)
        {
        }
        public virtual void Enter(AssignmentExpression assignmentExpression)
        {
        }
        public virtual void Enter(BaseReferenceExpression baseReferenceExpression)
        {
        }
        public virtual void Enter(BinaryOperatorExpression binaryOperatorExpression)
        {
        }
        public virtual void Enter(CastExpression castExpression)
        {
        }
        public virtual void Enter(ConditionalExpression conditionalExpression)
        {
        }
        public virtual void Enter(WrapperExpression wrapperExpression)
        {
        }
        public virtual void Enter(IndexerExpression indexerExpression)
        {
        }
        public virtual void Enter(InvocationExpression invocationExpression)
        {
        }
        public virtual void Enter(MemberReferenceExpression memberReferenceExpression)
        {
        }
        public virtual void Enter(MultichildExpression multichildExpression)
        {
        }
        public virtual void Enter(ObjectCreateExpression objectCreateExpression)
        {
        }
        public virtual void Enter(UnaryOperatorExpression unaryOperatorExpression)
        {
        }
        public virtual void Enter(VariableDeclarationExpression variableDeclarationExpression)
        {
        }

        public virtual void Exit(AnonymousMethodExpression anonymousMethodExpression)
        {
        }
        public virtual void Exit(ArrayCreationExpression arrayCreationExpression)
        {
        }
        public virtual void Exit(AssignmentExpression assignmentExpression)
        {
        }
        public virtual void Exit(BaseReferenceExpression baseReferenceExpression)
        {
        }
        public virtual void Exit(BinaryOperatorExpression binaryOperatorExpression)
        {
        }
        public virtual void Exit(CastExpression castExpression)
        {
        }
        public virtual void Exit(ConditionalExpression conditionalExpression)
        {
        }
        public virtual void Exit(WrapperExpression wrapperExpression)
        {
        }
        public virtual void Exit(IndexerExpression indexerExpression)
        {
        }
        public virtual void Exit(InvocationExpression invocationExpression)
        {
        }
        public virtual void Exit(MemberReferenceExpression memberReferenceExpression)
        {
        }
        public virtual void Exit(MultichildExpression multichildExpression)
        {
        }
        public virtual void Exit(ObjectCreateExpression objectCreateExpression)
        {
        }
        public virtual void Exit(UnaryOperatorExpression unaryOperatorExpression)
        {
        }
        public virtual void Exit(VariableDeclarationExpression variableDeclarationExpression)
        {
        }

        #endregion

        #region GeneralScope

        public virtual void Enter(NamespaceDeclaration namespaceDeclaration)
        {
        }
        public virtual void Enter(TypeDeclaration typeDeclaration)
        {
        }
        public virtual void Enter(UsingDeclaration usingDeclaration)
        {
        }

        public virtual void Exit(NamespaceDeclaration namespaceDeclaration)
        {
        }
        public virtual void Exit(TypeDeclaration typeDeclaration)
        {
        }
        public virtual void Exit(UsingDeclaration usingDeclaration)
        {
        }

        #endregion

        #region Literals

        public virtual void Enter(BinaryOperatorLiteral binaryOperatorLiteral)
        {
        }
        public virtual void Enter(BooleanLiteral booleanLiteral)
        {
        }
        public virtual void Enter(CommentLiteral commentLiteral)
        {
        }
        public virtual void Enter(FloatLiteral floatLiteral)
        {
        }
        public virtual void Enter(IdToken idToken)
        {
        }
        public virtual void Enter(IntLiteral intLiteral)
        {
        }
        public virtual void Enter(ModifierLiteral modifierLiteral)
        {
        }
        public virtual void Enter(NullLiteral nullLiteral)
        {
        }
        public virtual void Enter(ParameterModifierLiteral parameterModifierLiteral)
        {
        }
        public virtual void Enter(StringLiteral stringLiteral)
        {
        }
        public virtual void Enter(ThisReferenceToken thisReferenceLiteral)
        {
        }
        public virtual void Enter(TypeToken typeToken)
        {
        }
        public virtual void Enter(TypeTypeLiteral typeTypeToken)
        {
        }
        public virtual void Enter(UnaryOperatorLiteral unaryOperatorLiteral)
        {
        }

        public virtual void Exit(BinaryOperatorLiteral binaryOperatorLiteral)
        {
        }
        public virtual void Exit(BooleanLiteral booleanLiteral)
        {
        }
        public virtual void Exit(CommentLiteral commentLiteral)
        {
        }
        public virtual void Exit(FloatLiteral floatLiteral)
        {
        }
        public virtual void Exit(IdToken idToken)
        {
        }
        public virtual void Exit(IntLiteral intLiteral)
        {
        }
        public virtual void Exit(ModifierLiteral modifierLiteral)
        {
        }
        public virtual void Exit(NullLiteral nullLiteral)
        {
        }
        public virtual void Exit(ParameterModifierLiteral parameterModifierLiteral)
        {
        }
        public virtual void Exit(StringLiteral stringLiteral)
        {
        }
        public virtual void Exit(ThisReferenceToken thisReferenceLiteral)
        {
        }
        public virtual void Exit(TypeToken typeToken)
        {
        }
        public virtual void Exit(TypeTypeLiteral typeTypeToken)
        {
        }
        public virtual void Exit(UnaryOperatorLiteral unaryOperatorLiteral)
        {
        }

        #endregion

        #region Specific

        public virtual void Enter(AsExpression asExpression)
        {
        }
        public virtual void Enter(CheckedExpression checkedExpression)
        {
        }
        public virtual void Enter(CheckedStatement checkedStatement)
        {
        }
        public virtual void Enter(CSharpParameterDeclaration cSharpParameterDeclaration)
        {
        }
        public virtual void Enter(FixedStatement fixedStatement)
        {
        }
        public virtual void Enter(LockStatement lockStatement)
        {
        }
        public virtual void Enter(UnsafeStatement unsafeStatement)
        {
        }

        public virtual void Exit(AsExpression asExpression)
        {
        }
        public virtual void Exit(CheckedExpression checkedExpression)
        {
        }
        public virtual void Exit(CheckedStatement checkedStatement)
        {
        }
        public virtual void Exit(CSharpParameterDeclaration cSharpParameterDeclaration)
        {
        }
        public virtual void Exit(FixedStatement fixedStatement)
        {
        }
        public virtual void Exit(LockStatement lockStatement)
        {
        }
        public virtual void Exit(UnsafeStatement unsafeStatement)
        {
        }

        #endregion

        #region Statements

        public virtual void Enter(SwitchSection switchSection)
        {
        }
        public virtual void Enter(SwitchStatement switchStatement)
        {
        }
        public virtual void Enter(CatchClause catchClause)
        {
        }
        public virtual void Enter(TryCatchStatement tryCatchStatement)
        {
        }
        public virtual void Enter(BlockStatement blockStatement)
        {
        }
        public virtual void Enter(BreakStatement breakStatement)
        {
        }
        public virtual void Enter(ContinueStatement continueStatement)
        {
        }
        public virtual void Enter(DoWhileStatement doWhileStatement)
        {
        }
        public virtual void Enter(EmptyStatement emptyStatement)
        {
        }
        public virtual void Enter(ExpressionStatement expressionStatement)
        {
        }
        public virtual void Enter(ForeachStatement foreachStatement)
        {
        }
        public virtual void Enter(ForStatement forStatement)
        {
        }
        public virtual void Enter(GotoStatement gotoStatement)
        {
        }
        public virtual void Enter(IfElseStatement ifElseStatement)
        {
        }
        public virtual void Enter(ReturnStatement returnStatement)
        {
        }
        public virtual void Enter(ThrowStatement throwStatement)
        {
        }
        public virtual void Enter(TypeDeclarationStatement typeDeclarationStatement)
        {
        }
        public virtual void Enter(WhileStatement whileStatement)
        {
        }
        public virtual void Enter(WithStatement withStatement)
        {
        }
        public virtual void Enter(WrapperStatement wrapperStatement)
        {
        }

        public virtual void Exit(SwitchSection switchSection)
        {
        }
        public virtual void Exit(SwitchStatement switchStatement)
        {
        }
        public virtual void Exit(CatchClause catchClause)
        {
        }
        public virtual void Exit(TryCatchStatement tryCatchStatement)
        {
        }
        public virtual void Exit(BlockStatement blockStatement)
        {
        }
        public virtual void Exit(BreakStatement breakStatement)
        {
        }
        public virtual void Exit(ContinueStatement continueStatement)
        {
        }
        public virtual void Exit(DoWhileStatement doWhileStatement)
        {
        }
        public virtual void Exit(EmptyStatement emptyStatement)
        {
        }
        public virtual void Exit(ExpressionStatement expressionStatement)
        {
        }
        public virtual void Exit(ForeachStatement foreachStatement)
        {
        }
        public virtual void Exit(ForStatement forStatement)
        {
        }
        public virtual void Exit(GotoStatement gotoStatement)
        {
        }
        public virtual void Exit(IfElseStatement ifElseStatement)
        {
        }
        public virtual void Exit(ReturnStatement returnStatement)
        {
        }
        public virtual void Exit(ThrowStatement throwStatement)
        {
        }
        public virtual void Exit(TypeDeclarationStatement typeDeclarationStatement)
        {
        }
        public virtual void Exit(WhileStatement whileStatement)
        {
        }
        public virtual void Exit(WithStatement withStatement)
        {
        }
        public virtual void Exit(WrapperStatement wrapperStatement)
        {
        }

        #endregion

        #region TypeMembers

        public virtual void Enter(ConstructorDeclaration constructorDeclaration)
        {
        }
        public virtual void Enter(FieldDeclaration fieldDeclaration)
        {
        }
        public virtual void Enter(MethodDeclaration methodDeclaration)
        {
        }
        public virtual void Enter(ParameterDeclaration parameterDeclaration)
        {
        }
        public virtual void Enter(StatementDeclaration statementDeclaration)
        {
        }

        public virtual void Exit(ConstructorDeclaration constructorDeclaration)
        {
        }
        public virtual void Exit(FieldDeclaration fieldDeclaration)
        {
        }
        public virtual void Exit(MethodDeclaration methodDeclaration)
        {
        }
        public virtual void Exit(ParameterDeclaration parameterDeclaration)
        {
        }
        public virtual void Exit(StatementDeclaration statementDeclaration)
        {
        }

        #endregion

        #region Other

        public virtual void Enter(FileNode fileNode)
        {
        }
        public virtual void Enter(NotImplementedNode notImplementedNode)
        {
        }

        public virtual void Exit(FileNode fileNode)
        {
        }
        public virtual void Exit(NotImplementedNode notImplementedNode)
        {
        }

        #endregion

        #region Patterns

        public virtual void Enter(PatternNode patternVars)
        {
        }
        public virtual void Enter(PatternBooleanLiteral patternBooleanLiteral)
        {
        }
        public virtual void Enter(PatternComment patternComment)
        {
        }
        public virtual void Enter(PatternExpression patternExpression)
        {
        }
        public virtual void Enter(PatternExpressionInsideExpression patternExpressionInsideExpression)
        {
        }
        public virtual void Enter(PatternExpressionInsideStatement patternExpressionInsideStatement)
        {
        }
        public virtual void Enter(PatternExpressions patternExpressions)
        {
        }
        public virtual void Enter(PatternIdToken patternIdToken)
        {
        }
        public virtual void Enter(PatternIntLiteral patternIntLiteral)
        {
        }
        public virtual void Enter(PatternMultipleExpressions patternMultiExpressions)
        {
        }
        public virtual void Enter(PatternMultipleStatements patternMultiStatements)
        {
        }
        public virtual void Enter(PatternStatement patternStatement)
        {
        }
        public virtual void Enter(PatternStatements patternStatements)
        {
        }
        public virtual void Enter(PatternStringLiteral patternStringLiteral)
        {
        }
        public virtual void Enter(PatternTryCatchStatement patternTryCatchStatement)
        {
        }
        public virtual void Enter(PatternVarDef patternVarDef)
        {
        }
        public virtual void Enter(PatternVarRef patternVarRef)
        {
        }
        public virtual void Exit(PatternNode patternVars)
        {
        }
        public virtual void Exit(PatternBooleanLiteral patternBooleanLiteral)
        {
        }
        public virtual void Exit(PatternComment patternComment)
        {
        }
        public virtual void Exit(PatternExpression patternExpression)
        {
        }
        public virtual void Exit(PatternExpressionInsideExpression patternExpressionInsideExpression)
        {
        }
        public virtual void Exit(PatternExpressionInsideStatement patternExpressionInsideStatement)
        {
        }
        public virtual void Exit(PatternExpressions patternExpressions)
        {
        }
        public virtual void Exit(PatternIdToken patternIdToken)
        {
        }

        public virtual void Exit(PatternIntLiteral patternIntLiteral)
        {
        }
        public virtual void Exit(PatternMultipleExpressions patternMultiExpressions)
        {
        }
        public virtual void Exit(PatternMultipleStatements patternMultiStatements)
        {
        }
        public virtual void Exit(PatternStatement patternStatement)
        {
        }
        public virtual void Exit(PatternStatements patternStatements)
        {
        }
        public virtual void Exit(PatternStringLiteral patternStringLiteral)
        {
        }
        public virtual void Exit(PatternTryCatchStatement patternTryCatchStatement)
        {
        }
        public virtual void Exit(PatternVarDef patternVarDef)
        {
        }
        public virtual void Exit(PatternVarRef patternVarRef)
        {
        }

        #endregion

        #region Dsl

        public virtual void Enter(DslNode patternExpression)
        {
        }
        public virtual void Enter(LangCodeNode langCodeNode)
        {
        }

        public virtual void Exit(DslNode patternExpression)
        {
        }
        public virtual void Exit(LangCodeNode langCodeNode)
        {
        }

        #endregion

        #region 

        private void Visit(Node node)
        {
            if (node == null)
            {
                return;
            }

            Type type = node.GetType();
            PropertyInfo[] properties = ReflectionCache.GetClassProperties(type);
            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;
                TypeInfo propTypeInfo = propType.GetTypeInfo();
                if (propType == typeof(string) || propTypeInfo.IsValueType)
                {
                    // Ignore terminals
                }
                else if ((propTypeInfo.IsSubclassOf(typeof(Node)) || propType == typeof(Node)) && propType != typeof(FileNode))
                {
                    dynamic value = prop.GetValue(node);
                    if (value != null)
                    {
                        Enter(value);
                        Visit(value);
                        Exit(value);
                    }
                }
                else if (propTypeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
                {
                    Type itemType = propTypeInfo.GenericTypeArguments[0];
                    var collection = (IList)prop.GetValue(node);
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            dynamic nodeItem = item;
                            Enter(nodeItem);
                            Visit(nodeItem);
                            Exit(nodeItem);
                        }
                    }
                }
                else if (propTypeInfo == typeof(TextSpan) || propTypeInfo == typeof(ISymbol) ||
                         propTypeInfo == typeof(FileNode))
                {
                }
                else
                {
                    throw new NotImplementedException($"Property \"{prop}\" processing is not implemented via reflection");
                }
            }
        }

        #endregion
    }
}
