using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class UstSimplifier : UstVisitor<Ust>, ILoggable
    {
        private static PropertyCloner<Ust> propertyEnumerator = new PropertyCloner<Ust>
        {
            IgnoredProperties = new HashSet<string>() { nameof(Ust.Parent), nameof(Ust.Root) }
        };

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public RootUst Preprocess(RootUst ust)
        {
            var result = (RootUst)Preprocess((Ust)ust);
            return result;
        }

        public Ust Preprocess(Ust ustNode)
        {
            return Visit(ustNode);
        }

        public override Ust Visit(RootUst rootUst)
        {
            var newRoot = new RootUst(rootUst.SourceCodeFile, rootUst.Language);

            newRoot.SourceCodeFile = rootUst.SourceCodeFile;
            newRoot.Nodes = rootUst.Nodes.Select(node => Visit(node)).ToArray();
            newRoot.Comments = rootUst.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();

            newRoot.FillAscendants();
            return newRoot;
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
            Expression leftExpression = (Expression)Visit(binaryOperatorExpression.Left);
            BinaryOperatorLiteral op = (BinaryOperatorLiteral)Visit(binaryOperatorExpression.Operator);
            Expression rightExpression = (Expression)Visit(binaryOperatorExpression.Right);

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

        protected override Ust VisitChildren(Ust ust)
        {
            try
            {
                return propertyEnumerator.VisitProperties(ust, Visit);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(ust.Root?.SourceCodeFile?.FullPath ?? "", ex)
                {
                    TextSpan = ust.TextSpan
                });
                return null;
            }
        }
    }
}
