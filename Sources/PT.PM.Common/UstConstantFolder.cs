using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PT.PM.Common
{
    public class UstConstantFolder : ILoggable
    {
        public static readonly HashSet<Type> FoldingTypes = new HashSet<Type>
        {
            typeof(ArrayCreationExpression),
            typeof(BinaryOperatorExpression),
            typeof(UnaryOperatorExpression),
            typeof(TupleCreateExpression)
        };

        private readonly Dictionary<TextSpan, FoldResult> foldedResults = new Dictionary<TextSpan, FoldResult>();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool TryGetOrFold(Ust ust, out FoldResult result)
        {
            if (ust == null || !FoldingTypes.Contains(ust.GetType()))
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
                case TupleCreateExpression tupleCreateExpression:
                    return TryFoldTupleCreateExpression(tupleCreateExpression);
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
                (bool)arrayCreationExpression.Initializers?.All(i => i is StringLiteral))
            {
                var value = new StringBuilder();
                var textSpans = new List<TextSpan>(arrayCreationExpression.Initializers.Count);

                foreach (var expression in arrayCreationExpression.Initializers)
                {
                    var stringLiteral = (StringLiteral) expression;
                    AppendText(value, stringLiteral);
                    textSpans.Add(stringLiteral.TextSpan);
                }

                return new FoldResult(value.ToString(), textSpans);
            }

            return null;
        }

        private FoldResult TryFoldTupleCreateExpression(TupleCreateExpression tupleCreateExpression)
        {
            var sb = new StringBuilder();
            var textSpans = new List<TextSpan>(tupleCreateExpression.Initializers.Count);
            foreach (var initializer in tupleCreateExpression.Initializers)
            {
                if (initializer is StringLiteral stringLiteral)
                {
                    AppendText(sb, stringLiteral);
                    textSpans.Add(stringLiteral.TextSpan);
                }
            }
            return sb.Length > 0
                ? new FoldResult(sb.ToString(), textSpans)
                : null;
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
                        leftStringBuilder = (StringBuilder)leftValue;
                    }
                    leftStringBuilder.Append(rightString);

                    FoldResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFold, rightFold, leftStringBuilder);

                    if (Logger.LogLevel >= LogLevel.Debug)
                    {
                        Logger.LogDebug(
                            $"Strings {binaryOperatorExpression} concatenated to \"{leftStringBuilder}\" at {binaryOperatorExpression.TextSpan}");
                    }

                    return result;
                }
            }
            else
            {
                if (TryFoldIntBinaryOperatorExpression(leftValue, rightValue, op, out BigInteger bigInt))
                {
                    FoldResult result = ProcessBinaryExpression(binaryOperatorExpression, leftFold, rightFold, bigInt);

                    if (Logger.LogLevel >= LogLevel.Debug)
                    {
                        Logger.LogDebug($"Arithmetic expression {binaryOperatorExpression} folded to {bigInt} at {binaryOperatorExpression.TextSpan}");
                    }

                    return result;
                }
            }

            return null;
        }

        private bool TryFoldIntBinaryOperatorExpression(object left, object right,
            BinaryOperatorLiteral op, out BigInteger result)
        {
            bool folded = true;
            if (!((left is long || left is int || left is BigInteger)
                && (right is long || right is int || right is BigInteger)))
            {
                return false;
            }

            BigInteger leftBigInt = left is int leftInt ? (BigInteger)leftInt
                : left is long leftLong ? (BigInteger)leftLong
                : (BigInteger)left;

            BigInteger rightBigInt = right is int rightInt ? (BigInteger)rightInt
                : right is long rightLong ? (BigInteger)rightLong
                : (BigInteger)right;

            switch (op.BinaryOperator)
            {
                case BinaryOperator.Plus:
                    result = leftBigInt + rightBigInt;
                    break;
                case BinaryOperator.Minus:
                    result = leftBigInt - rightBigInt;
                    break;
                case BinaryOperator.Multiply:
                    result = leftBigInt * rightBigInt;
                    break;
                case BinaryOperator.Divide:
                    if (rightBigInt == 0)
                    {
                        folded = false;
                    }
                    else
                    {
                        result = leftBigInt / rightBigInt;
                    }
                    break;
                case BinaryOperator.Mod:
                    if (rightBigInt == 0)
                    {
                        folded = false;
                    }
                    else
                    {
                        result = leftBigInt % rightBigInt;
                    }
                    break;
                case BinaryOperator.BitwiseAnd:
                    result = leftBigInt & rightBigInt;
                    break;
                case BinaryOperator.BitwiseOr:
                    result = leftBigInt | rightBigInt;
                    break;
                case BinaryOperator.BitwiseXor:
                    result = leftBigInt ^ rightBigInt;
                    break;
                default:
                    folded = false;
                    break;
            }
            return folded;
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
                object negativeValue = null;
                switch (foldResult.Value)
                {
                    case double doubleVal:
                        negativeValue = -doubleVal;
                        break;
                    case int intVal:
                        negativeValue = -intVal;
                        break;
                    case long longVal:
                        negativeValue = -longVal;
                        break;
                    case BigInteger bigIntVal:
                        negativeValue = -bigIntVal;
                        break;
                }

                if (negativeValue != null)
                {
                    return ProcessUnaryExpression(unaryOperatorExpression, foldResult, negativeValue);
                }
            }

            return null;
        }

        private FoldResult TryFoldToken(Token token)
        {
            if (token is StringLiteral stringLiteral)
            {
                return new FoldResult(stringLiteral.TextValue, stringLiteral.TextSpan);
            }

            if (token is IntLiteral intLiteral)
            {
                return new FoldResult(intLiteral.Value, intLiteral.TextSpan);
            }

            if (token is BigIntLiteral bigIntLiteral)
            {
                return new FoldResult(bigIntLiteral.Value, bigIntLiteral.TextSpan);
            }

            if (token is LongLiteral longLiteral)
            {
                return new FoldResult(longLiteral.Value, longLiteral.TextSpan);
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

        private void AppendText(StringBuilder builder, StringLiteral literal)
        {
            if (literal.Text is null)
            {
                string data = literal.CurrentSourceFile.Data;
                for (int i = literal.TextSpan.Start; i < literal.TextSpan.End; i++)
                {
                    builder.Append(data[i]);
                }
            }
            else
            {
                builder.Append(literal.Text);
            }
        }
    }
}