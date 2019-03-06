using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PT.PM.Common
{
    public class UstConstantFolder : ILoggable
    {
        private static readonly HashSet<Type> foldingTypes = new HashSet<Type>
        {
            typeof(ArrayCreationExpression),
            typeof(BinaryOperatorExpression),
            typeof(UnaryOperatorExpression)
        };

        private readonly Dictionary<TextSpan, FoldResult> foldedResults = new Dictionary<TextSpan, FoldResult>();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool TryGetOrFold(Ust ust, out FoldResult result)
        {
            if (ust == null || !foldingTypes.Contains(ust.GetType()))
            {
                result = null;
                return false;
            }

            lock (ust)
            {
                if (foldedResults.TryGetValue(ust.TextSpan, out result))
                {
                    return result != null;
                }

                result = TryGetOrFold(ust);

                NormalizeAndAdd(ust, ref result);
            }

            return result != null;
        }

        private FoldResult TryGetOrFold(Ust ust)
        {
            if (ust != null && foldedResults.TryGetValue(ust.TextSpan, out FoldResult result))
            {
                return result;
            }

            switch (ust)
            {
                case ArrayCreationExpression arrayCreationExpression:
                    return TryFoldArrayCreationExpression(arrayCreationExpression);
                case BinaryOperatorExpression binaryOperatorExpression:
                    return TryFoldBinaryOperatorExpression(binaryOperatorExpression);
                case UnaryOperatorExpression unaryOperatorExpression:
                    return TryFoldUnaryOperatorExpression(unaryOperatorExpression);
                case Token token:
                    return TryFoldToken(token);
                // TODO: parenthesis
                default:
                    return null;
            }
        }

        private FoldResult TryFoldArrayCreationExpression(ArrayCreationExpression arrayCreationExpression)
        {
            if (arrayCreationExpression.Type?.TypeText == "char" &&
                arrayCreationExpression.Initializers.Count > 0 &&
                (bool) arrayCreationExpression.Initializers?.All(i => i is StringLiteral))
            {
                var value = new StringBuilder();
                var textSpans = new List<TextSpan>(arrayCreationExpression.Initializers.Count + 1)
                {
                    arrayCreationExpression.Type.TextSpan
                };

                foreach (Expression expression in arrayCreationExpression.Initializers)
                {
                    var stringLiteral = (StringLiteral) expression;
                    value.Append(stringLiteral.Text);
                    textSpans.Add(stringLiteral.TextSpan);
                }

                return new FoldResult(value.ToString(), textSpans);
            }

            return null;
        }

        private FoldResult TryFoldBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            FoldResult leftFold = TryGetOrFold(binaryOperatorExpression.Left);
            if (leftFold == null)
            {
                NormalizeAndAdd(binaryOperatorExpression.Left, ref leftFold);
                return null;
            }

            FoldResult rightFold = TryGetOrFold(binaryOperatorExpression.Right);
            if (rightFold == null)
            {
                NormalizeAndAdd(binaryOperatorExpression.Right, ref rightFold);
                return null;
            }

            object leftValue = leftFold.Value;
            object rightValue = rightFold.Value;
            BinaryOperatorLiteral op = binaryOperatorExpression.Operator;

            if (leftValue is string || leftValue is StringBuilder)
            {
                if (op.BinaryOperator == BinaryOperator.Plus || op.BinaryOperator == BinaryOperator.Concat)
                {
                    // Use StringBuilder instead of immutable strings and concatenation
                    string rightString = rightValue.ToString();

                    StringBuilder leftStringBuilder;
                    if (leftValue is string leftString)
                    {
                        leftStringBuilder = new StringBuilder((leftString.Length + rightString.Length) * 2);
                        leftStringBuilder.Append(leftString);
                    }
                    else
                    {
                        leftStringBuilder = (StringBuilder) leftValue;
                    }
                    leftStringBuilder.Append(rightString);

                    FoldResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFold, rightFold, leftStringBuilder);

                    if (Logger.IsLogDebugs)
                    {
                        Logger.LogDebug(
                            $"Strings {binaryOperatorExpression} concatenated to \"{leftStringBuilder}\" at {binaryOperatorExpression.TextSpan}");
                    }

                    return result;
                }
            }
            else if (leftValue is long leftInt && rightValue is long rightInt)
            {
                long resultInt = 0;
                bool folded = true;
                try
                {
                    checked
                    {
                        switch (op.BinaryOperator)
                        {
                            case BinaryOperator.Plus:
                                resultInt = leftInt + rightInt;
                                break;
                            case BinaryOperator.Minus:
                                resultInt = leftInt - rightInt;
                                break;
                            case BinaryOperator.Multiply:
                                resultInt = leftInt * rightInt;
                                break;
                            case BinaryOperator.Divide:
                                if (rightInt == 0)
                                {
                                    folded = false;
                                }
                                else
                                {
                                    resultInt = leftInt / rightInt;
                                }
                                break;
                            case BinaryOperator.Mod:
                                if (rightInt == 0)
                                {
                                    folded = false;
                                }
                                else
                                {
                                    resultInt = leftInt % rightInt;
                                }
                                break;
                            case BinaryOperator.BitwiseAnd:
                                resultInt = leftInt & rightInt;
                                break;
                            case BinaryOperator.BitwiseOr:
                                resultInt = leftInt | rightInt;
                                break;
                            case BinaryOperator.BitwiseXor:
                                resultInt = leftInt ^ rightInt;
                                break;
                            default:
                                folded = false;
                                break;
                        }

                        if (folded)
                        {
                            FoldResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFold, rightFold, resultInt);

                            if (Logger.IsLogDebugs)
                            {
                                Logger.LogDebug($"Arithmetic expression {binaryOperatorExpression} folded to {resultInt} at {binaryOperatorExpression.TextSpan}");
                            }

                            return result;
                        }
                    }
                }
                catch (Exception ex) when (!(ex is ThreadAbortException))
                {
                    if (Logger.IsLogDebugs)
                    {
                        Logger.LogDebug($"Error while constant folding: {ex}");
                    }
                }
            }

            return null;
        }

        private FoldResult TryFoldUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            FoldResult foldResult = TryGetOrFold(unaryOperatorExpression.Expression);

            if (foldResult == null)
            {
                NormalizeAndAdd(unaryOperatorExpression.Expression, ref foldResult);
                return null;
            }

            if (unaryOperatorExpression.Operator.UnaryOperator == UnaryOperator.Minus)
            {
                if (foldResult.Value is long longValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldResult, -longValue);
                }

                if (foldResult.Value is double doubleValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldResult, -doubleValue);
                }
            }

            return null;
        }

        private FoldResult TryFoldToken(Token token)
        {
            if (token is StringLiteral stringLiteral)
            {
                return new FoldResult(stringLiteral.Text, stringLiteral.TextSpan);
            }

            if (token is IntLiteral intLiteral)
            {
                return new FoldResult(intLiteral.Value, intLiteral.TextSpan);
            }

            if (token is FloatLiteral floatLiteral)
            {
                return new FoldResult(floatLiteral.Value, floatLiteral.TextSpan);
            }

            if (token is BooleanLiteral booleanLiteral)
            {
                return new FoldResult(booleanLiteral.Value, booleanLiteral.TextSpan);
            }

            return null;
        }

        private FoldResult ProcessBinaryExpression(BinaryOperatorExpression binaryOperatorExpression,
            FoldResult leftFold, FoldResult rightFold, object foldedBinaryValue)
        {
            var textSpans = leftFold.TextSpans;
            if (!(foldedBinaryValue is string || foldedBinaryValue is StringBuilder))
            {
                textSpans.Add(binaryOperatorExpression.Operator.TextSpan); // FIXME: workaround for correct PatternStringRegexLiteral processing
            }
            textSpans.AddRange(rightFold.TextSpans);

            return new FoldResult(foldedBinaryValue, leftFold.TextSpans);
        }

        private FoldResult ProcessUnaryExpression(UnaryOperatorExpression unaryOperatorExpression, FoldResult foldResult,
            object resultValue)
        {
            var textSpans = foldResult.TextSpans;
            textSpans.Add(unaryOperatorExpression.Operator.TextSpan);

            return new FoldResult(resultValue, textSpans);
        }

        private void NormalizeAndAdd(Ust ust, ref FoldResult result)
        {
            if (result?.Value is StringBuilder stringBuilder)
            {
                result = new FoldResult(stringBuilder.ToString(), result.TextSpans);
            }

            result?.TextSpans.Sort();
            foldedResults[ust.TextSpan] = result;
        }
    }
}