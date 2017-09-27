using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes.TypeMembers;
using PT.PM.Matching.Patterns;

namespace PT.PM.Matching
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

            if (leftExpression is StringLiteral leftString &&
                rightExpression is StringLiteral rightString)
            {
                if (op.BinaryOperator == BinaryOperator.Plus)
                {
                    string resultText = leftString.Text + rightString.Text;
                    result = new StringLiteral
                    {
                        Text = resultText,
                        Root = binaryOperatorExpression.Root,
                        TextSpan = leftExpression.TextSpan.Union(rightExpression.TextSpan)
                    };
                    Logger.LogDebug($"Strings {binaryOperatorExpression} has been concatenated to \"{resultText}\" at {result.TextSpan}");
                }
            }
            else if (leftExpression is IntLiteral leftInt &&
                     rightExpression is IntLiteral rightInt)
            {
                long leftValue = leftInt.Value;
                long rightValue = rightInt.Value;
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
            }

            return result;
        }

        public override Ust Visit(IfElseStatement ifElseStatement)
        {
            return VisitChildren(ifElseStatement);
        }

        public override Ust Visit(UnaryOperatorExpression unaryOperatorExpression)
        {
            UnaryOperatorLiteral op = unaryOperatorExpression.Operator;
            Expression expression = unaryOperatorExpression.Expression;

            if (op.UnaryOperator == UnaryOperator.Minus)
            {
                if (expression is IntLiteral intLiteral)
                {
                    long intValue = intLiteral.Value;
                    Ust result = new IntLiteral
                    {
                        Value = -intValue,
                        Root = unaryOperatorExpression.Root,
                        TextSpan = op.TextSpan.Union(expression.TextSpan)
                    };
                    Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-intValue} at {result.TextSpan}");
                    return result;
                }

                if (expression is FloatLiteral floatLiteral)
                {
                    double doubleValue = floatLiteral.Value;
                    Ust result = new FloatLiteral
                    {
                        Value = -doubleValue,
                        Root = unaryOperatorExpression.Root,
                        TextSpan = op.TextSpan.Union(expression.TextSpan)
                    };
                    Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-doubleValue} at {result.TextSpan}");
                    return result;
                }
            }

            return VisitChildren(unaryOperatorExpression);
        }

        public Ust Visit(PatternArgs patternExpressions)
        {
            // #* #* ... #* -> #*
            List<PatternBase> args = patternExpressions.Args
                .Select(item => (PatternBase)Visit(item)).ToList();
            int index = 0;
            while (index < args.Count)
            {
                if (args[index] is PatternMultipleExpressions &&
                    index + 1 < args.Count &&
                    args[index + 1] is PatternMultipleExpressions)
                {
                    args.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
            var result = new PatternArgs(args);
            return result;
        }

        public Ust Visit(PatternStatements patternStatements)
        {
            return VisitChildren(patternStatements);
        }

        public Ust Visit(PatternOr patternOr)
        {
            if (patternOr.Alternatives.Count == 1)
            {
                return Visit(patternOr.Alternatives[0]);
            }

            IEnumerable<PatternBase> exprs = patternOr.Alternatives
                .Select(e => (PatternBase)Visit(e))
                .OrderBy(e => e);
            return new PatternOr(exprs, patternOr.TextSpan);
        }

        public Ust Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public Ust Visit(PatternComment patternComment)
        {
            return VisitChildren(patternComment);
        }

        public Ust Visit(PatternAnyExpression patternAnyExpression)
        {
            return VisitChildren(patternAnyExpression);
        }

        public Ust Visit(PatternExpressionInside patternExpressionInsideExpression)
        {
            return VisitChildren(patternExpressionInsideExpression);
        }

        public Ust Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public Ust Visit(PatternIdRegexToken patternIdRegexToken)
        {
            if (patternIdRegexToken.Id.All(
                c => char.IsLetterOrDigit(c) || c == '_'))
            {
                return new PatternIdToken(
                    patternIdRegexToken.Id,
                    patternIdRegexToken.TextSpan);
            }
            else
            {
                return new PatternIdRegexToken(
                    patternIdRegexToken.Id,
                    patternIdRegexToken.TextSpan);
            }
        }

        public Ust Visit(PatternIntRangeLiteral patternIntLiteral)
        {
            if (patternIntLiteral.MinValue == patternIntLiteral.MaxValue)
            {
                return new PatternIntLiteral(
                    patternIntLiteral.MinValue,
                    patternIntLiteral.TextSpan);
            }
            else
            {
                return new PatternIntRangeLiteral(
                    patternIntLiteral.MinValue,
                    patternIntLiteral.MaxValue,
                    patternIntLiteral.TextSpan);
            }
        }

        public override Ust Visit(MultichildExpression multichildExpression)
        {
            return VisitChildren(multichildExpression);
        }

        public Ust Visit(PatternMultipleExpressions patternMultiExpressions)
        {
            return VisitChildren(patternMultiExpressions);
        }

        public Ust Visit(PatternStringRegexLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public Ust Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public Ust Visit(PatternAnd patternAnd)
        {
            if (patternAnd.Expressions.Count == 1)
            {
                return Visit(patternAnd.Expressions[0]);
            }

            IEnumerable<PatternBase> exprs = patternAnd.Expressions
                .Select(e => (PatternBase)Visit(e))
                .OrderBy(e => e);
            return new PatternAnd(exprs, patternAnd.TextSpan);
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

        public Ust Visit(PatternVar patternVar)
        {
            return VisitChildren(patternVar);
        }

        public Ust Visit(PatternAnonymousMethodExpression patternAnonymousMethodExpression)
        {
            return VisitChildren(patternAnonymousMethodExpression);
        }

        public Ust Visit(PatternAssignmentExpression patternAssignmentExpression)
        {
            return VisitChildren(patternAssignmentExpression);
        }

        public Ust Visit(PatternBaseReferenceExpression patternBaseReferenceExpression)
        {
            return VisitChildren(patternBaseReferenceExpression);
        }

        public Ust Visit(PatternBinaryOperatorExpression patternBinaryOperatorExpression)
        {
            return VisitChildren(patternBinaryOperatorExpression);
        }

        public Ust Visit(PatternIndexerExpression patternIndexerExpression)
        {
            return VisitChildren(patternIndexerExpression);
        }

        public Ust Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public Ust Visit(PatternInvocationExpression patternInvocationExpression)
        {
            return VisitChildren(patternInvocationExpression);
        }

        public Ust Visit(PatternMemberReferenceExpression patternMemberReferenceExpression)
        {
            return VisitChildren(patternMemberReferenceExpression);
        }

        public Ust Visit(PatternNullLiteral patternNullLiteral)
        {
            return VisitChildren(patternNullLiteral);
        }

        public Ust Visit(PatternObjectCreateExpression patternObjectCreateExpression)
        {
            return VisitChildren(patternObjectCreateExpression);
        }

        public Ust Visit(PatternParameterDeclaration patternParameterDeclaration)
        {
            return VisitChildren(patternParameterDeclaration);
        }

        public Ust Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
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
