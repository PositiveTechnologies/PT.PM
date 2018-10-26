
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common
{
    public class UstConstantFolder : ILoggable
    {
        private readonly Dictionary<Ust, FoldResult> foldedResults = new Dictionary<Ust, FoldResult>(UstRefComparer.Instance);

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool TryFold(Ust ust, out FoldResult result)
        {
            if (!(ust is ArrayCreationExpression || ust is BinaryOperatorExpression || ust is UnaryOperatorExpression ||
                  ust is Token))
            {
                result = null;
                return false;
            }

            if (foldedResults.TryGetValue(ust, out result))
            {
                return result != null;
            }

            result = TryFold(ust);
            foldedResults.Add(ust, result);

            if (result != null)
            {
                result.TextSpans.Sort();
                return true;
            }

            return false;
        }

        private FoldResult TryFold(Ust ust)
        {
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
               (arrayCreationExpression.Initializers?.All(i => i is StringLiteral) ?? false))
            {
                var value = new StringBuilder();
                var textSpans = new List<TextSpan>(arrayCreationExpression.Initializers.Count + 1);
                
                textSpans.Add(arrayCreationExpression.Type.TextSpan);
                
                foreach (StringLiteral stringLiteral in arrayCreationExpression.Initializers)
                {
                    value.Append(stringLiteral.Text);
                    textSpans.Add(stringLiteral.TextSpan);
                }
                
                return new FoldResult(value.ToString(), textSpans);
            }

            return null;
        }
        
        private FoldResult TryFoldBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            FoldResult leftFold = TryFold(binaryOperatorExpression.Left);
            FoldResult rightFold = TryFold(binaryOperatorExpression.Right);
            object leftValue = leftFold?.Value;
            object rightValue = rightFold?.Value;
            BinaryOperatorLiteral op = binaryOperatorExpression.Operator;

            if (leftValue is string leftString && rightFold != null)
            {
                if (op.BinaryOperator == BinaryOperator.Plus || op.BinaryOperator == BinaryOperator.Concat)
                {
                    // Use StringBuilder instead of immutable strings and concatenation
                    string resultText = leftString + rightValue;
                    FoldResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFold, rightFold, resultText);

                    if (Logger.IsLogDebugs)
                    {
                        Logger.LogDebug(
                            $"Strings {binaryOperatorExpression} concatenated to \"{resultText}\" at {binaryOperatorExpression.TextSpan}");
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
                                    break;
                                }
                                resultInt = leftInt / rightInt;
                                break;
                            case BinaryOperator.Mod:
                                resultInt = leftInt % rightInt;
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

            // Otherwise store result of the previous folding
            foldedResults.Add(binaryOperatorExpression.Left, leftFold);
            foldedResults.Add(binaryOperatorExpression.Right, rightFold);
            
            return null;
        }

        private FoldResult TryFoldUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            FoldResult foldResult = TryFold(unaryOperatorExpression.Expression);
            object value = foldResult?.Value;
            
            if (unaryOperatorExpression.Operator.UnaryOperator == UnaryOperator.Minus)
            {
                if (value is long longValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldResult, -longValue);
                }

                if (value is double doubleValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldResult, -doubleValue);
                }
            }

            foldedResults.Add(unaryOperatorExpression.Expression, foldResult);

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
            if (!(foldedBinaryValue is string))
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
    }
}