using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Exceptions;
using PT.PM.Patterns.Nodes;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using PT.PM.Patterns;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.UstPreprocessing
{
    public class UstSimplifier : UstVisitor<Ust>, IUstPatternVisitor<Ust>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public RootUst Preprocess(RootUst ust)
        {
            var result = (RootUst)Visit((dynamic)ust);
            return result;
        }

        public Ust Preprocess(Ust ustNode)
        {
            return Visit((dynamic)ustNode);
        }

        public override Ust Visit(RootUst rootUstNode)
        {
            var newRoot = new RootUst(rootUstNode.SourceCodeFile, rootUstNode.Language);

            newRoot.SourceCodeFile = rootUstNode.SourceCodeFile;
            newRoot.Nodes = rootUstNode.Nodes.Select(node => Visit(node)).ToArray();
            newRoot.Comments = rootUstNode.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();

            newRoot.FillAscendants();
            return newRoot;
        }

        public Ust Visit(PatternRootUst patternNode)
        {
            var newPattern = new PatternRootUst(patternNode.SourceCodeFile);

            newPattern.SourceCodeFile = patternNode.SourceCodeFile;
            newPattern.Nodes = patternNode.Nodes.Select(node => Visit(node)).ToArray();
            newPattern.Comments = patternNode.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();
            newPattern.Languages = new HashSet<Language>(patternNode.Languages);

            List<PatternVarDef> vars = patternNode.Vars.Select(v => (PatternVarDef)Visit(v)).ToList();
            vars.Sort();
            newPattern.Vars = vars;

            newPattern.FillAscendants();
            return newPattern;
        }

        public override Ust Visit(Ust ustNode)
        {
            if (ustNode == null)
            {
                return null;
            }
            return Visit((dynamic)ustNode);
        }

        public override Ust Visit(EntityDeclaration entityDeclaration)
        {
            if (entityDeclaration == null)
            {
                return null;
            }
            return Visit((dynamic)entityDeclaration);
        }

        public override Ust Visit(Statement statement)
        {
            if (statement == null)
            {
                return null;
            }
            return Visit((dynamic)statement);
        }

        public override Ust Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            return Visit((dynamic)expression);
        }

        public override Ust Visit(Token literal)
        {
            if (literal == null)
            {
                return null;
            }
            return Visit((dynamic)literal);
        }

        public override Ust Visit(ArrayCreationExpression arrayCreationExpression)
        {
            if (arrayCreationExpression.Initializers?.All(i => i is StringLiteral) ?? false)
            {
                string value = String.Concat(
                    arrayCreationExpression.Initializers.OfType<StringLiteral>().Select(expr => expr.Text));
                return new StringLiteral(value, arrayCreationExpression.TextSpan);
            }
            return VisitChildren(arrayCreationExpression);
        }

        public override Ust Visit(BinaryOperatorExpression binaryOperatorExpression)
        {
            Expression result = null;
            Expression leftExpression = Visit((dynamic)binaryOperatorExpression.Left);
            BinaryOperatorLiteral op = Visit((dynamic)binaryOperatorExpression.Operator);
            Expression rightExpression = Visit((dynamic)binaryOperatorExpression.Right);

            if (leftExpression.Kind == UstKind.StringLiteral &&
                rightExpression.Kind == UstKind.StringLiteral)
            {
                string leftValue = ((StringLiteral)leftExpression).Text;
                string rightValue = ((StringLiteral)rightExpression).Text;
                if (op.BinaryOperator == BinaryOperator.Plus)
                {
                    string resultText = leftValue + rightValue;
                    result = new StringLiteral
                    {
                        Text = resultText,
                        Root = binaryOperatorExpression.Root,
                        TextSpan = leftExpression.TextSpan.Union(rightExpression.TextSpan)
                    };
                    Logger.LogDebug($"Strings {binaryOperatorExpression} has been concatenated to \"{resultText}\" at {result.TextSpan}");
                }
            }
            else if (leftExpression.Kind == UstKind.IntLiteral &&
                rightExpression.Kind == UstKind.IntLiteral)
            {
                long leftValue = ((IntLiteral)leftExpression).Value;
                long rightValue = ((IntLiteral)rightExpression).Value;
                long resultValue = 0;
                bool folded = true;
                try
                {
                    checked
                    {
                        switch (op.BinaryOperator)
                        {
                            case BinaryOperator.Plus:
                                resultValue = leftValue + rightValue;
                                break;
                            case BinaryOperator.Minus:
                                resultValue = leftValue - rightValue;
                                break;
                            case BinaryOperator.Multiply:
                                resultValue = leftValue * rightValue;
                                break;
                            case BinaryOperator.Divide:
                                resultValue = leftValue / rightValue;
                                break;
                            case BinaryOperator.Mod:
                                resultValue = leftValue % rightValue;
                                break;
                            case BinaryOperator.BitwiseAnd:
                                resultValue = leftValue & rightValue;
                                break;
                            case BinaryOperator.BitwiseOr:
                                resultValue = leftValue | rightValue;
                                break;
                            case BinaryOperator.BitwiseXor:
                                resultValue = leftValue ^ rightValue;
                                break;
                            default:
                                folded = false;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    folded = false;
                    Logger.LogDebug($"Error while constant folding: {ex}");
                }
                if (folded)
                {
                    result = new IntLiteral
                    {
                        Value = resultValue,
                        Root = binaryOperatorExpression.Root,
                        TextSpan = leftExpression.TextSpan.Union(rightExpression.TextSpan)
                    };
                    Logger.LogDebug($"Arithmetic expression {binaryOperatorExpression} has been folded to {resultValue} at {result.TextSpan}");
                }
            }

            if (result == null)
            {
                result = new BinaryOperatorExpression(leftExpression, op, rightExpression,
                    new TextSpan(binaryOperatorExpression.TextSpan));
                leftExpression.Parent = result;
                rightExpression.Parent = result;
                op.Parent = result;
            }

            return result;
        }

        // Unify Statement to BlockStatement.
        public override Ust Visit(IfElseStatement ifElseStatement)
        {
            Expression condition = (Expression)Visit(ifElseStatement.Condition);
            BlockStatement trueStatement = ConvertToBlockStatement((Statement)Visit(ifElseStatement.TrueStatement));
            BlockStatement falseStatement = ConvertToBlockStatement((Statement)Visit(ifElseStatement.FalseStatement));
            var result = new IfElseStatement(condition, trueStatement, ifElseStatement.TextSpan);
            result.Condition.Parent = result;
            result.TrueStatement.Parent = result;
            if (result.FalseStatement != null)
            {
                result.FalseStatement.Parent = result;
            }
            return result;
        }

        public override Ust Visit(UnaryOperatorExpression unaryOperatorExpression)
        {
            UnaryOperatorLiteral op = unaryOperatorExpression.Operator;
            Expression ex = unaryOperatorExpression.Expression;

            if (op.UnaryOperator == UnaryOperator.Minus)
            {
                if (ex.Kind == UstKind.IntLiteral)
                {
                    long intValue = ((IntLiteral)ex).Value;
                    Ust result = new IntLiteral
                    {
                        Value = -intValue,
                        Root = unaryOperatorExpression.Root,
                        TextSpan = op.TextSpan.Union(ex.TextSpan)
                    };
                    Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-intValue} at {result.TextSpan}");
                    return result;
                }

                if (ex.Kind == UstKind.FloatLiteral)
                {
                    double doubleValue = ((FloatLiteral)ex).Value;
                    Ust result = new FloatLiteral
                    {
                        Value = -doubleValue,
                        Root = unaryOperatorExpression.Root,
                        TextSpan = op.TextSpan.Union(ex.TextSpan)
                    };
                    Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-doubleValue} at {result.TextSpan}");
                    return result;
                }
            }

            return VisitChildren(unaryOperatorExpression);
        }

        private BlockStatement ConvertToBlockStatement(Statement statement)
        {
            BlockStatement result;
            if (statement == null)
            {
                result = null;
            }
            else if (statement.Kind == UstKind.BlockStatement)
            {
                result = (BlockStatement)statement;
            }
            else
            {
                result = new BlockStatement(new Statement[] { statement }, statement.TextSpan);
                statement.Parent = result;
            }
            return result;
        }

        public Ust Visit(PatternExpressions patternExpressions)
        {
            // #* #* ... #* -> #*
            List<Expression> collection = patternExpressions.Collection
                .Select(item => (Expression)Visit(item)).ToList();
            int index = 0;
            while (index < collection.Count)
            {
                if (collection[index].Kind == UstKind.PatternMultipleExpressions &&
                    index + 1 < collection.Count &&
                    collection[index + 1].Kind == UstKind.PatternMultipleExpressions)
                {
                    collection.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
            var result = new PatternExpressions
            {
                Collection = new List<Expression>(collection)
            };
            return result;
        }

        public Ust Visit(PatternStatements patternStatements)
        {
            // ... ... ... -> ...
            List<Statement> collection = patternStatements.Statements
                .Select(item => (Statement)Visit(item)).ToList();
            int index = 0;
            while (index < collection.Count)
            {
                if (collection[index].Kind == UstKind.PatternMultipleStatements &&
                    index + 1 < collection.Count &&
                    collection[index + 1].Kind == UstKind.PatternMultipleStatements)
                {
                    collection.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
            var result = new PatternStatements
            {
                Statements = new List<Statement>(collection)
            };
            return result;
        }

        public Ust Visit(PatternVarDef patternVarDef)
        {
            List<Expression> vars = patternVarDef.Values.Select(v => (Expression)Visit(v)).ToList();
            vars.Sort();
            return new PatternVarDef(patternVarDef.Id, vars, patternVarDef.TextSpan);
        }

        public Ust Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public Ust Visit(PatternComment patternComment)
        {
            return VisitChildren(patternComment);
        }

        public Ust Visit(PatternExpression patternExpression)
        {
            return VisitChildren(patternExpression);
        }

        public Ust Visit(PatternExpressionInsideNode patternExpressionInsideExpression)
        {
            return VisitChildren(patternExpressionInsideExpression);
        }

        public Ust Visit(PatternExpressionInsideStatement patternExpressionInsideStatement)
        {
            return VisitChildren(patternExpressionInsideStatement);
        }

        public Ust Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public Ust Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public override Ust Visit(MultichildExpression multichildExpression)
        {
            return VisitChildren(multichildExpression);
        }

        public Ust Visit(PatternMultipleExpressions patternMultiExpressions)
        {
            return VisitChildren(patternMultiExpressions);
        }

        public Ust Visit(PatternMultipleStatements patternMultiStatements)
        {
            return VisitChildren(patternMultiStatements);
        }

        public Ust Visit(PatternStatement patternStatement)
        {
            return VisitChildren(patternStatement);
        }

        public Ust Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public Ust Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public Ust Visit(PatternVarRef patternVarRef)
        {
            return VisitChildren(patternVarRef);
        }

        public Ust Visit(PatternAnd patternAnd)
        {
            return VisitChildren(patternAnd);
        }

        public Ust Visit(PatternNot patternNot)
        {
            return VisitChildren(patternNot);
        }

        public Ust Visit(PatternClassDeclaration patternClassDeclaration)
        {
            return VisitChildren(patternClassDeclaration);
        }

        public Ust Visit(PatternMethodDeclaration patternMethodDeclaration)
        {
            return VisitChildren(patternMethodDeclaration);
        }

        public Ust Visit(PatternVarOrFieldDeclaration patternVarOrFieldDeclaration)
        {
            return VisitChildren(patternVarOrFieldDeclaration);
        }

        protected override Ust VisitChildren(Ust ustNode)
        {
            try
            {
                if (ustNode == null)
                {
                    return null;
                }

                Type type = ustNode.GetType();
                PropertyInfo[] properties = ReflectionCache.GetClassProperties(type);

                var result = (Ust)Activator.CreateInstance(type);
                foreach (PropertyInfo prop in properties)
                {
                    Type propType = prop.PropertyType;
                    if (propType.IsValueType || propType == typeof(string))
                    {
                        prop.SetValue(result, prop.GetValue(ustNode));
                    }
                    else if (prop.Name == nameof(Ust.Parent) || prop.Name == nameof(Ust.Root))
                    {
                        continue;
                    }
                    else if (typeof(Ust).IsAssignableFrom(propType))
                    {
                        Ust getValue = (Ust)prop.GetValue(ustNode);
                        if (getValue != null)
                        {
                            Ust setValue = Visit(getValue);
                            prop.SetValue(result, setValue);
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
                            foreach (object item in sourceCollection)
                            {
                                var ustNodeItem = item as Ust;
                                if (ustNodeItem != null)
                                {
                                    var destUstNodeItem = Visit(ustNodeItem);
                                    destCollection.Add(destUstNodeItem);
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
                        continue;
                    }
                    else
                    {
                        throw new NotImplementedException($"Property \"{prop}\" processing is not implemented via reflection");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(ustNode.Root?.SourceCodeFile?.FullPath ?? "", ex) { TextSpan = ustNode.TextSpan });
                return null;
            }
        }
    }
}
