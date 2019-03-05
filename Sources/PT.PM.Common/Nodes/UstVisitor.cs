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
    public class UstVisitor<T> : IUstVisitor<T>
    {
        public virtual T Visit(Ust ust)
        {
            if (ust == null)
            {
                return default;
            }
            return Visit((dynamic)ust);
        }

        public virtual T Visit(ArgsUst argsUst)
        {
            return VisitChildren(argsUst);
        }

        public virtual T Visit(ArgumentExpression argumentExpression)
        {
            return VisitChildren(argumentExpression);
        }

        public virtual T Visit(EntitiesUst entitiesUst)
        {
            return VisitChildren(entitiesUst);
        }

        public virtual T Visit(AnonymousMethodExpression anonymousMethodExpression)
        {
            return VisitChildren(anonymousMethodExpression);
        }

        public virtual T Visit(AnonymousObjectExpression anonymousObjectExpression)
        {
            return VisitChildren(anonymousObjectExpression);
        }

        public virtual T Visit(ArrayCreationExpression arrayCreationExpression)
        {
            return VisitChildren(arrayCreationExpression);
        }

        public virtual T Visit(ArrayPatternExpression arrayPatternExpression)
        {
            return VisitChildren(arrayPatternExpression);
        }

        public virtual T Visit(AssignmentExpression assignmentExpression)
        {
            return VisitChildren(assignmentExpression);
        }

        public virtual T Visit(BaseReferenceToken baseReferenceExpression)
        {
            return VisitChildren(baseReferenceExpression);
        }

        public virtual T Visit(BinaryOperatorExpression binaryOperatorExpression)
        {
            return VisitChildren(binaryOperatorExpression);
        }

        public virtual T Visit(CastExpression castExpression)
        {
            return VisitChildren(castExpression);
        }

        public virtual T Visit(ConditionalExpression conditionalExpression)
        {
            return VisitChildren(conditionalExpression);
        }

        public virtual T Visit(WrapperExpression wrapperExpression)
        {
            return VisitChildren(wrapperExpression);
        }

        public virtual T Visit(IndexerExpression indexerExpression)
        {
            return VisitChildren(indexerExpression);
        }

        public virtual T Visit(InvocationExpression invocationExpression)
        {
            return VisitChildren(invocationExpression);
        }

        public virtual T Visit(MemberReferenceExpression memberReferenceExpression)
        {
            return VisitChildren(memberReferenceExpression);
        }

        public virtual T Visit(MultichildExpression multichildExpression)
        {
            return VisitChildren(multichildExpression);
        }

        public virtual T Visit(ObjectCreateExpression objectCreateExpression)
        {
            return VisitChildren(objectCreateExpression);
        }

        public virtual T Visit(UnaryOperatorExpression unaryOperatorExpression)
        {
            return VisitChildren(unaryOperatorExpression);
        }

        public virtual T Visit(VariableDeclarationExpression variableDeclarationExpression)
        {
            return VisitChildren(variableDeclarationExpression);
        }

        public virtual T Visit(YieldExpression yieldExpression)
        {
            return VisitChildren(yieldExpression);
        }

        public virtual T Visit(CommaExpression colonExpression)
        {
            return VisitChildren(colonExpression);
        }

        public virtual T Visit(NamespaceDeclaration namespaceDeclaration)
        {
            return VisitChildren(namespaceDeclaration);
        }

        public virtual T Visit(TypeDeclaration typeDeclaration)
        {
            return VisitChildren(typeDeclaration);
        }

        public virtual T Visit(UsingDeclaration usingDeclaration)
        {
            return VisitChildren(usingDeclaration);
        }

        public virtual T Visit(BinaryOperatorLiteral binaryOperatorLiteral)
        {
            return VisitChildren(binaryOperatorLiteral);
        }

        public virtual T Visit(BooleanLiteral booleanLiteral)
        {
            return VisitChildren(booleanLiteral);
        }

        public virtual T Visit(CommentLiteral commentLiteral)
        {
            return VisitChildren(commentLiteral);
        }

        public virtual T Visit(FloatLiteral floatLiteral)
        {
            return VisitChildren(floatLiteral);
        }

        public virtual T Visit(IdToken idToken)
        {
            return VisitChildren(idToken);
        }

        public virtual T Visit(IntLiteral intLiteral)
        {
            return VisitChildren(intLiteral);
        }

        public virtual T Visit(BigIntLiteral bigIntLiteral)
        {
            return VisitChildren(bigIntLiteral);
        }

        public virtual T Visit(LongLiteral longLiteral)
        {
            return VisitChildren(longLiteral);
        }

        public virtual T Visit(ModifierLiteral modifierLiteral)
        {
            return VisitChildren(modifierLiteral);
        }

        public virtual T Visit(NullLiteral nullLiteral)
        {
            return VisitChildren(nullLiteral);
        }

        public virtual T Visit(InOutModifierLiteral parameterModifierLiteral)
        {
            return VisitChildren(parameterModifierLiteral);
        }

        public virtual T Visit(StringLiteral stringLiteral)
        {
            return VisitChildren(stringLiteral);
        }

        public virtual T Visit(ThisReferenceToken thisReferenceLiteral)
        {
            return VisitChildren(thisReferenceLiteral);
        }

        public virtual T Visit(TypeToken typeToken)
        {
            return VisitChildren(typeToken);
        }

        public virtual T Visit(TypeTypeLiteral typeTypeLiteral)
        {
            return VisitChildren(typeTypeLiteral);
        }

        public virtual T Visit(UnaryOperatorLiteral unaryOperatorLiteral)
        {
            return VisitChildren(unaryOperatorLiteral);
        }

        public virtual T Visit(AsExpression asExpression)
        {
            return VisitChildren(asExpression);
        }

        public virtual T Visit(CheckedExpression checkedExpression)
        {
            return VisitChildren(checkedExpression);
        }

        public virtual T Visit(DebuggerStatement debuggerStatement)
        {
            return VisitChildren(debuggerStatement);
        }

        public virtual T Visit(SqlQuery sqlQueryStatement)
        {
            return VisitChildren(sqlQueryStatement);
        }

        public virtual T Visit(SqlBlockStatement sqlBlockStatement)
        {
            return VisitChildren(sqlBlockStatement);
        }

        public virtual T Visit(QueryArgs queryParameters)
        {
            return VisitChildren(queryParameters);
        }

        public virtual T Visit(FixedStatement fixedStatement)
        {
            return VisitChildren(fixedStatement);
        }

        public virtual T Visit(LabelStatement labelStatement)
        {
            return VisitChildren(labelStatement);
        }

        public virtual T Visit(LockStatement lockStatement)
        {
            return VisitChildren(lockStatement);
        }

        public virtual T Visit(UnsafeStatement unsafeStatement)
        {
            return VisitChildren(unsafeStatement);
        }

        public virtual T Visit(SwitchSection switchSection)
        {
            return VisitChildren(switchSection);
        }

        public virtual T Visit(SwitchStatement switchStatement)
        {
            return VisitChildren(switchStatement);
        }

        public virtual T Visit(CatchClause catchClause)
        {
            return VisitChildren(catchClause);
        }

        public virtual T Visit(TryCatchStatement tryCatchStatement)
        {
            return VisitChildren(tryCatchStatement);
        }

        public virtual T Visit(BlockStatement blockStatement)
        {
            return VisitChildren(blockStatement);
        }

        public virtual T Visit(BreakStatement breakStatement)
        {
            return VisitChildren(breakStatement);
        }

        public virtual T Visit(ContinueStatement continueStatement)
        {
            return VisitChildren(continueStatement);
        }

        public virtual T Visit(DoWhileStatement doWhileStatement)
        {
            return VisitChildren(doWhileStatement);
        }

        public virtual T Visit(EmptyStatement emptyStatement)
        {
            return VisitChildren(emptyStatement);
        }

        public virtual T Visit(ExpressionStatement expressionStatement)
        {
            return VisitChildren(expressionStatement);
        }

        public virtual T Visit(ForeachStatement foreachStatement)
        {
            return VisitChildren(foreachStatement);
        }

        public virtual T Visit(ForStatement forStatement)
        {
            return VisitChildren(forStatement);
        }

        public virtual T Visit(GotoStatement gotoStatement)
        {
            return VisitChildren(gotoStatement);
        }

        public virtual T Visit(IfElseStatement ifElseStatement)
        {
            return VisitChildren(ifElseStatement);
        }

        public virtual T Visit(ReturnStatement returnStatement)
        {
            return VisitChildren(returnStatement);
        }

        public virtual T Visit(ThrowStatement throwStatement)
        {
            return VisitChildren(throwStatement);
        }

        public virtual T Visit(TypeDeclarationStatement typeDeclarationStatement)
        {
            return VisitChildren(typeDeclarationStatement);
        }

        public virtual T Visit(WhileStatement whileStatement)
        {
            return VisitChildren(whileStatement);
        }

        public virtual T Visit(WithStatement withStatement)
        {
            return VisitChildren(withStatement);
        }

        public virtual T Visit(WrapperStatement wrapperStatement)
        {
            return VisitChildren(wrapperStatement);
        }

        public virtual T Visit(ConstructorDeclaration constructorDeclaration)
        {
            return VisitChildren(constructorDeclaration);
        }

        public virtual T Visit(FieldDeclaration fieldDeclaration)
        {
            return VisitChildren(fieldDeclaration);
        }

        public virtual T Visit(PropertyDeclaration propertyDeclaration)
        {
            return VisitChildren(propertyDeclaration);
        }

        public virtual T Visit(MethodDeclaration methodDeclaration)
        {
            return VisitChildren(methodDeclaration);
        }

        public virtual T Visit(ParameterDeclaration parameterDeclaration)
        {
            return VisitChildren(parameterDeclaration);
        }

        public virtual T Visit(StatementDeclaration statementDeclaration)
        {
            return VisitChildren(statementDeclaration);
        }

        public virtual T Visit(RootUst fileNode)
        {
            return VisitChildren(fileNode);
        }

        public virtual T Visit(Collection collection)
        {
            return VisitChildren(collection);
        }

        public T Visit(TupleCreateExpression tupleCreateExpression)
        {
            return VisitChildren(tupleCreateExpression);
        }

        protected virtual T VisitChildren(Ust ust)
        {
            if (ust == null)
            {
                return default;
            }
            foreach (var children in ust.Children)
            {
                if (children != null)
                {
                    T result = Visit((dynamic)children);
                    if (ust.Children.Length == 1)
                    {
                        return result;
                    }
                }
            }

            return default;
        }
    }
} 
