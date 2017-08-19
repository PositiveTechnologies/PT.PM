using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Collections;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.GeneralScope;
using PT.PM.Common.Nodes.Specific;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Statements.Switch;
using PT.PM.Common.Nodes.Statements.TryCatchFinally;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Common.Symbols;
using PT.PM.Dsl;
using PT.PM.Patterns.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PT.PM.UstPreprocessing
{
    public class UstVisitor : IUstVisitor<UstNode>
    {
        protected FileNode fileNode;

        public virtual UstNode Visit(UstNode ustNode)
        {
            if (ustNode == null)
            {
                return null;
            }
            return Visit((dynamic)ustNode);
        }

        public virtual UstNode Visit(EntityDeclaration entityDeclaration)
        {
            if (entityDeclaration == null)
            {
                return null;
            }
            return Visit((dynamic)entityDeclaration);
        }

        public virtual UstNode Visit(Statement statement)
        {
            if (statement == null)
            {
                return null;
            }
            return Visit((dynamic)statement);
        }

        public virtual UstNode Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            return Visit((dynamic)expression);
        }

        public virtual UstNode Visit(Token literal)
        {
            if (literal == null)
            {
                return null;
            }
            return Visit((dynamic)literal);
        }

        public virtual UstNode Visit(AnonymousMethodExpression anonymousMethodExpression)
        {
            return VisitChildren(anonymousMethodExpression);
        }

        public virtual UstNode Visit(ArgsNode argsNode)
        {
            return VisitChildren(argsNode);
        }

        public virtual UstNode Visit(ArrayCreationExpression arrayCreationExpression)
        {
            return VisitChildren(arrayCreationExpression);
        }

        public virtual UstNode Visit(AssignmentExpression assignmentExpression)
        {
            return VisitChildren(assignmentExpression);
        }

        public virtual UstNode Visit(BaseReferenceExpression baseReferenceExpression)
        {
            return VisitChildren(baseReferenceExpression);
        }

        public virtual UstNode Visit(BinaryOperatorExpression binaryOperatorExpression)
        {
            return VisitChildren(binaryOperatorExpression);
        }

        public virtual UstNode Visit(BinaryOperatorLiteral binaryOperatorLiteral)
        {
            return VisitChildren(binaryOperatorLiteral);
        }

        public virtual UstNode Visit(BlockStatement blockStatement)
        {
            return VisitChildren(blockStatement);
        }

        public virtual UstNode Visit(BooleanLiteral booleanLiteral)
        {
            return VisitChildren(booleanLiteral);
        }

        public virtual UstNode Visit(BreakStatement breakStatement)
        {
            return VisitChildren(breakStatement);
        }

        public virtual UstNode Visit(GotoStatement gotoStatement)
        {
            return VisitChildren(gotoStatement);
        }

        public virtual UstNode Visit(CastExpression castExpression)
        {
            return VisitChildren(castExpression);
        }

        public virtual UstNode Visit(CatchClause catchClause)
        {
            return VisitChildren(catchClause);
        }

        public virtual UstNode Visit(TryCatchStatement tryCatchStatement)
        {
            return VisitChildren(tryCatchStatement);
        }

        public virtual UstNode Visit(CommentLiteral commentLiteral)
        {
            return VisitChildren(commentLiteral);
        }

        public virtual UstNode Visit(ConditionalExpression conditionalExpression)
        {
            return VisitChildren(conditionalExpression);
        }

        public virtual UstNode Visit(ConstructorDeclaration constructorDeclaration)
        {
            return VisitChildren(constructorDeclaration);
        }

        public virtual UstNode Visit(ContinueStatement continueStatement)
        {
            return VisitChildren(continueStatement);
        }

        public virtual UstNode Visit(DoWhileStatement doWhileStatement)
        {
            return VisitChildren(doWhileStatement);
        }

        public virtual UstNode Visit(EmptyStatement emptyStatement)
        {
            return VisitChildren(emptyStatement);
        }

        public virtual UstNode Visit(EntitiesNode entitiesNode)
        {
            return VisitChildren(entitiesNode);
        }

        public virtual UstNode Visit(ExpressionStatement expressionStatement)
        {
            return VisitChildren(expressionStatement);
        }

        public virtual UstNode Visit(FieldDeclaration fieldDeclaration)
        {
            return VisitChildren(fieldDeclaration);
        }

        public virtual UstNode Visit(FileNode fileNode)
        {
            return VisitChildren(fileNode);
        }

        public virtual UstNode Visit(FloatLiteral floatLiteral)
        {
            return VisitChildren(floatLiteral);
        }

        public virtual UstNode Visit(ForeachStatement foreachStatement)
        {
            return VisitChildren(foreachStatement);
        }

        public virtual UstNode Visit(ForStatement forStatement)
        {
            return VisitChildren(forStatement);
        }

        public virtual UstNode Visit(IdToken idToken)
        {
            return VisitChildren(idToken);
        }

        public virtual UstNode Visit(IfElseStatement ifElseStatement)
        {
            return VisitChildren(ifElseStatement);
        }

        public virtual UstNode Visit(WrapperExpression wrapperExpression)
        {
            return VisitChildren(wrapperExpression);
        }

        public virtual UstNode Visit(IndexerExpression indexerExpression)
        {
            return VisitChildren(indexerExpression);
        }

        public virtual UstNode Visit(IntLiteral intLiteral)
        {
            return VisitChildren(intLiteral);
        }

        public virtual UstNode Visit(InvocationExpression invocationExpression)
        {
            return VisitChildren(invocationExpression);
        }

        public virtual UstNode Visit(MemberReferenceExpression memberReferenceExpression)
        {
            return VisitChildren(memberReferenceExpression);
        }

        public virtual UstNode Visit(MethodDeclaration methodDeclaration)
        {
            return VisitChildren(methodDeclaration);
        }

        public virtual UstNode Visit(ModifierLiteral modifierLiteral)
        {
            return VisitChildren(modifierLiteral);
        }

        public virtual UstNode Visit(MultichildExpression multichildExpression)
        {
            return VisitChildren(multichildExpression);
        }

        public virtual UstNode Visit(NamespaceDeclaration namespaceDeclaration)
        {
            return VisitChildren(namespaceDeclaration);
        }

        public virtual UstNode Visit(NotImplementedNode notImplementedNode)
        {
            return VisitChildren(notImplementedNode);
        }

        public virtual UstNode Visit(NullLiteral nullLiteral)
        {
            return VisitChildren(nullLiteral);
        }

        public virtual UstNode Visit(ObjectCreateExpression objectCreateExpression)
        {
            return VisitChildren(objectCreateExpression);
        }

        public virtual UstNode Visit(ParameterDeclaration parameterDeclaration)
        {
            return VisitChildren(parameterDeclaration);
        }

        public virtual UstNode Visit(ParameterModifierLiteral parameterModifierLiteral)
        {
            return VisitChildren(parameterModifierLiteral);
        }

        public UstNode Visit(DslNode dslNode)
        {
            return VisitChildren(dslNode);
        }

        public UstNode Visit(LangCodeNode langCodeNode)
        {
            return VisitChildren(langCodeNode);
        }

        public virtual UstNode Visit(ReturnStatement returnStatement)
        {
            return VisitChildren(returnStatement);
        }

        public virtual UstNode Visit(StatementDeclaration statementDeclaration)
        {
            return VisitChildren(statementDeclaration);
        }

        public virtual UstNode Visit(StringLiteral stringLiteral)
        {
            return VisitChildren(stringLiteral);
        }

        public virtual UstNode Visit(SwitchSection switchSection)
        {
            return VisitChildren(switchSection);
        }

        public virtual UstNode Visit(SwitchStatement switchStatement)
        {
            return VisitChildren(switchStatement);
        }

        public virtual UstNode Visit(ThisReferenceToken thisReferenceLiteral)
        {
            return VisitChildren(thisReferenceLiteral);
        }

        public virtual UstNode Visit(ThrowStatement throwStatement)
        {
            return VisitChildren(throwStatement);
        }

        public virtual UstNode Visit(TypeDeclaration typeDeclaration)
        {
            return VisitChildren(typeDeclaration);
        }

        public virtual UstNode Visit(TypeDeclarationStatement typeDeclarationStatement)
        {
            return VisitChildren(typeDeclarationStatement);
        }

        public virtual UstNode Visit(TypeToken typeToken)
        {
            return VisitChildren(typeToken);
        }

        public virtual UstNode Visit(TypeTypeLiteral typeTypeToken)
        {
            return VisitChildren(typeTypeToken);
        }

        public virtual UstNode Visit(UnaryOperatorExpression unaryOperatorExpression)
        {
            return VisitChildren(unaryOperatorExpression);
        }

        public virtual UstNode Visit(UnaryOperatorLiteral unaryOperatorLiteral)
        {
            return VisitChildren(unaryOperatorLiteral);
        }

        public virtual UstNode Visit(AsExpression asExpression)
        {
            return VisitChildren(asExpression);
        }

        public virtual UstNode Visit(CheckedExpression checkedExpression)
        {
            return VisitChildren(checkedExpression);
        }

        public virtual UstNode Visit(CheckedStatement checkedStatement)
        {
            return VisitChildren(checkedStatement);
        }

        public virtual UstNode Visit(CSharpParameterDeclaration cSharpParameterDeclaration)
        {
            return VisitChildren(cSharpParameterDeclaration);
        }

        public virtual UstNode Visit(FixedStatement fixedStatement)
        {
            return VisitChildren(fixedStatement);
        }

        public virtual UstNode Visit(LockStatement lockStatement)
        {
            return VisitChildren(lockStatement);
        }

        public virtual UstNode Visit(UnsafeStatement unsafeStatement)
        {
            return VisitChildren(unsafeStatement);
        }

        public virtual UstNode Visit(UsingDeclaration usingDeclaration)
        {
            return VisitChildren(usingDeclaration);
        }

        public virtual UstNode Visit(VariableDeclarationExpression variableDeclarationExpression)
        {
            return VisitChildren(variableDeclarationExpression);
        }

        public virtual UstNode Visit(WhileStatement whileStatement)
        {
            return VisitChildren(whileStatement);
        }

        public virtual UstNode Visit(WithStatement withStatement)
        {
            return VisitChildren(withStatement);
        }

        public virtual UstNode Visit(WrapperStatement wrapperStatement)
        {
            return VisitChildren(wrapperStatement);
        }

        public virtual UstNode Visit(PatternNode patternNode)
        {
            return VisitChildren(patternNode);
        }

        public virtual UstNode Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public virtual UstNode Visit(PatternExpression patternExpression)
        {
            return VisitChildren(patternExpression);
        }

        public virtual UstNode Visit(PatternExpressionInsideExpression patternExpressionInsideExpression)
        {
            return VisitChildren(patternExpressionInsideExpression);
        }

        public virtual UstNode Visit(PatternExpressionInsideStatement patternExpressionInsideStatement)
        {
            return VisitChildren(patternExpressionInsideStatement);
        }

        public virtual UstNode Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public virtual UstNode Visit(PatternMultipleExpressions patternMultiExpressions)
        {
            return VisitChildren(patternMultiExpressions);
        }

        public virtual UstNode Visit(PatternStatement patternStatement)
        {
            return VisitChildren(patternStatement);
        }

        public virtual UstNode Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public virtual UstNode Visit(PatternVarDef patternVarDef)
        {
            return VisitChildren(patternVarDef);
        }

        public virtual UstNode Visit(PatternVarRef patternVarRef)
        {
            return VisitChildren(patternVarRef);
        }

        public virtual UstNode Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public virtual UstNode Visit(PatternStatements patternStatements)
        {
            return VisitChildren(patternStatements);
        }

        public virtual UstNode Visit(PatternMultipleStatements patternMultiStatements)
        {
            return VisitChildren(patternMultiStatements);
        }

        public virtual UstNode Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public virtual UstNode Visit(PatternExpressions patternExpressions)
        {
            return VisitChildren(patternExpressions);
        }

        public virtual UstNode Visit(PatternComment patternComment)
        {
            return VisitChildren(patternComment);
        }

        protected virtual UstNode VisitChildren(UstNode ustNode)
        {
            if (ustNode == null)
            {
                return null;
            }

            Type type = ustNode.GetType();
            PropertyInfo[] properties = ReflectionCache.GetClassProperties(type);

            var result = (UstNode)Activator.CreateInstance(type);
            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;
                if (propType.IsValueType || propType == typeof(string))
                {
                    prop.SetValue(result, prop.GetValue(ustNode));
                }
                else if (typeof(UstNode).IsAssignableFrom(propType) && propType.Name != nameof(UstNode.Parent))
                {
                    UstNode getValue = (UstNode)prop.GetValue(ustNode);
                    UstNode setValue = VisitNodeOrIgnoreFileNode(getValue);
                    prop.SetValue(result, setValue);
                    if (setValue != null)
                    {
                        setValue.Parent = result;
                    }
                }
                else if (prop.Name.StartsWith(nameof(IAbsoluteLocationMatching.MatchedLocation)))
                {
                    // ignore matched locations.
                }
                else if (propType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    Type itemType = propType.GetGenericArguments()[0];
                    var sourceCollection = (IEnumerable<object>)prop.GetValue(ustNode);
                    IList destCollection = null;
                    if (sourceCollection != null)
                    {
                        destCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                        foreach (var item in sourceCollection)
                        {
                            var ustNodeItem = item as UstNode;
                            if (ustNodeItem != null)
                            {
                                var destUstNodeItem = VisitNodeOrIgnoreFileNode(ustNodeItem);
                                destCollection.Add(destUstNodeItem);
                                if (destUstNodeItem != null)
                                {
                                    destUstNodeItem.Parent = result;
                                }
                            }
                            else
                            {
                                destCollection.Add(item);
                            }
                        }
                    }
                    prop.SetValue(result, destCollection);
                }
                else if (propType == typeof(Regex))
                {
                    // ignore regex as they assignment via strings.
                }
                else if (propType == typeof(ISymbol))
                {
                    // ignore Symbols.
                }
                else
                {
                    throw new NotImplementedException($"Property \"{prop}\" processing is not implemented via reflection");
                }
            }

            return result;
        }

        protected UstNode VisitNodeOrIgnoreFileNode(UstNode node)
        {
            if (node == null)
            {
                return null;
            }
            else if (node.NodeType != NodeType.FileNode)
            {
                return Visit(node); // Prevent endless recursion.
            }
            else
            {
                if (fileNode == null)
                {
                    var fileNode = (FileNode)node;
                    this.fileNode = new FileNode(fileNode?.FileName?.Text, fileNode?.FileData);
                }
                return fileNode;
            }
        }
    }
}
