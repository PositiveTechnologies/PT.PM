
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common
{
    public enum FoldingResultType
    {
        Folded,
        NotFolded,
        FoldedChild, // for optimization
    }
    
    public class FoldingResult
    {
        public FoldingResultType Type { get; }
        
        public object Value { get; private set; }
        
        public List<TextSpan> TextSpans { get; private set; }

        public static FoldingResult CreateFolded(object value, TextSpan textSpan)
        {
            return new FoldingResult(FoldingResultType.Folded)
            {
                Value = value,
                TextSpans = new List<TextSpan> {textSpan}
            };
        }
        
        public static FoldingResult CreateFolded(object value, List<TextSpan> textSpans)
        {
            return new FoldingResult(FoldingResultType.Folded)
            {
                Value = value,
                TextSpans = textSpans
            };
        }

        public static FoldingResult CreateNotFolded() => new FoldingResult(FoldingResultType.NotFolded);
        
        public static FoldingResult CreateFoldedChild() => new FoldingResult(FoldingResultType.FoldedChild);

        private FoldingResult(FoldingResultType type) => Type = type;
    }
    
    public class UstConstantFolder : ILoggable
    {
        private readonly Dictionary<Ust, FoldingResult> foldedResults = new Dictionary<Ust, FoldingResult>(UstRefComparer.Instance);

        public ILogger Logger { get; set; } = DummyLogger.Instance;
        
        public bool TryFold(Ust ust, out FoldingResult result)
        {
            if (foldedResults.TryGetValue(ust, out result))
            {
                return result?.Type == FoldingResultType.Folded;
            }

            result = TryFold(ust);
            result.TextSpans?.Sort();
            foldedResults.Add(ust, result);
            
            return result?.Type == FoldingResultType.Folded;
        }

        private FoldingResult TryFold(Ust ust)
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
                    return FoldingResult.CreateNotFolded();
            }
        }

        private FoldingResult TryFoldArrayCreationExpression(ArrayCreationExpression arrayCreationExpression)
        {
            if (arrayCreationExpression.Type?.TypeText == "char" &&
               (arrayCreationExpression.Initializers?.All(i => i is StringLiteral) ?? false))
            {
                var value = new StringBuilder();
                var textSpans = new List<TextSpan>(arrayCreationExpression.Initializers.Count + 1);
                
                textSpans.Add(arrayCreationExpression.Type.TextSpan);
                foldedResults.Add(arrayCreationExpression.Type, FoldingResult.CreateFoldedChild());
                
                foreach (StringLiteral stringLiteral in arrayCreationExpression.Initializers)
                {
                    value.Append(stringLiteral.Text);
                    textSpans.Add(stringLiteral.TextSpan);
                    foldedResults.Add(stringLiteral, FoldingResult.CreateFoldedChild());
                }
                
                FoldingResult result = FoldingResult.CreateFolded(value.ToString(), textSpans);
                return result;
            }

            return FoldingResult.CreateNotFolded();
        }
        
        private FoldingResult TryFoldBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            FoldingResult leftFolding = TryFold(binaryOperatorExpression.Left);
            FoldingResult rightFolding = TryFold(binaryOperatorExpression.Right);
            object leftValue = leftFolding?.Value;
            object rightValue = rightFolding?.Value;
            BinaryOperatorLiteral op = binaryOperatorExpression.Operator;

            if (leftValue is string leftString && rightFolding != null)
            {
                if (op.BinaryOperator == BinaryOperator.Plus || op.BinaryOperator == BinaryOperator.Concat)
                {
                    FoldingResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFolding, rightFolding, leftString + rightValue);

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
                            FoldingResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFolding, rightFolding, resultInt);
                            
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
            foldedResults.Add(binaryOperatorExpression.Left, leftFolding);
            foldedResults.Add(binaryOperatorExpression.Right, rightFolding);
            return FoldingResult.CreateNotFolded();
        }

        private FoldingResult TryFoldUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            FoldingResult foldingResult = TryFold(unaryOperatorExpression.Expression);
            object value = foldingResult?.Value;
            
            if (unaryOperatorExpression.Operator.UnaryOperator == UnaryOperator.Minus)
            {
                if (value is long longValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldingResult, -longValue);
                }

                if (value is double doubleValue)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldingResult, -doubleValue);
                }
            }
            
            foldedResults.Add(unaryOperatorExpression.Expression, foldingResult);
            return FoldingResult.CreateNotFolded();
        }

        private FoldingResult TryFoldToken(Token token)
        {
            if (token is StringLiteral stringLiteral)
            {
                return FoldingResult.CreateFolded(stringLiteral.Text, stringLiteral.TextSpan);
            }
            
            if (token is IntLiteral intLiteral)
            {
                return FoldingResult.CreateFolded(intLiteral.Value, intLiteral.TextSpan);
            }

            if (token is FloatLiteral floatLiteral)
            {
                return FoldingResult.CreateFolded(floatLiteral.Value, floatLiteral.TextSpan);
            }

            if (token is BooleanLiteral booleanLiteral)
            {
                return FoldingResult.CreateFolded(booleanLiteral.Value, booleanLiteral.TextSpan);
            }

            return FoldingResult.CreateNotFolded();
        }
        
        private FoldingResult ProcessBinaryExpression(BinaryOperatorExpression binaryOperatorExpression,
            FoldingResult leftFolding, FoldingResult rightFolding, object foldedBinaryValue)
        {
            var textSpans = leftFolding.TextSpans;
            textSpans.Add(binaryOperatorExpression.TextSpan);
            textSpans.AddRange(rightFolding.TextSpans);

            FoldingResult result = FoldingResult.CreateFolded(foldedBinaryValue, leftFolding.TextSpans);
            foldedResults.Add(binaryOperatorExpression.Left, FoldingResult.CreateFoldedChild());
            foldedResults.Add(binaryOperatorExpression.Right, FoldingResult.CreateFoldedChild());
            
            return result;
        }
        
        private FoldingResult ProcessUnaryExpression(UnaryOperatorExpression unaryOperatorExpression, FoldingResult foldingResult,
            object resultValue)
        {
            var textSpans = foldingResult.TextSpans;
            textSpans.Add(unaryOperatorExpression.Operator.TextSpan);
            
            FoldingResult result = FoldingResult.CreateFolded(resultValue, textSpans);
            foldedResults.Add(unaryOperatorExpression.Expression, FoldingResult.CreateFoldedChild());
            
            return result;
        }
    }
}