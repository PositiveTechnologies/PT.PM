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
using System.Text;
using System.Threading;

namespace PT.PM
{
    public class UstSimplifier : UstVisitor<Ust>, ILoggable
    {
        private static PropertyCloner<Ust> propertyEnumerator = new PropertyCloner<Ust>
        {
            IgnoredProperties = new HashSet<string>() { nameof(Ust.Parent), nameof(Ust.Root) }
        };

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public RootUst Simplify(RootUst ust)
        {
            var result = (RootUst)Simplify((Ust)ust);
            return result;
        }

        public Ust Simplify(Ust ustNode)
        {
            return Visit(ustNode);
        }

        public override Ust Visit(RootUst rootUst)
        {
            var newRoot = new RootUst(rootUst.SourceCodeFile, rootUst.Language)
            {
                SourceCodeFile = rootUst.SourceCodeFile,
                Nodes = rootUst.Nodes.Select(node => Visit(node)).ToArray(),
                Comments = rootUst.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray()
            };

            newRoot.FillAscendants();
            return newRoot;
        }

        public override Ust Visit(ArrayCreationExpression arrayCreationExpression)
        {
            Ust result;

            if (arrayCreationExpression.Type?.TypeText == "char" &&
                (arrayCreationExpression.Initializers?.All(i => i is StringLiteral) ?? false))
            {
                var value = new StringBuilder();
                var textSpans = new List<TextSpan>(arrayCreationExpression.Initializers.Count);
                foreach (StringLiteral stringLiteral in arrayCreationExpression.Initializers)
                {
                    value.Append(stringLiteral.Text);
                    textSpans.Add(stringLiteral.TextSpan);
                }
                result = new StringLiteral(value.ToString(), textSpans.Union())
                {
                    InitialTextSpans = textSpans
                };
            }
            else
            {
                result = VisitChildren(arrayCreationExpression);
            }

            return result;
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
                if (op.BinaryOperator == BinaryOperator.Plus || op.BinaryOperator == BinaryOperator.Concat)
                {
                    string resultText = leftString.Text + rightString.Text;
                    result = new StringLiteral(resultText);

                    if (Logger.IsLogDebugs)
                    {
                        Logger.LogDebug($"Strings {binaryOperatorExpression} concatenated to \"{resultText}\" at {binaryOperatorExpression.TextSpan}");
                    }
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

                        if (folded)
                        {
                            result = new IntLiteral(resultValue);
                            if (Logger.IsLogDebugs)
                            {
                                Logger.LogDebug($"Arithmetic expression {binaryOperatorExpression} folded to {resultValue} at {binaryOperatorExpression.TextSpan}");
                            }
                        }
                    }
                }
                catch (Exception ex) when (!(ex is ThreadAbortException))
                {
                    Logger.LogDebug($"Error while constant folding: {ex}");
                }
            }

            if (result != null)
            {
                List<TextSpan> textSpans = leftExpression.GetRealTextSpans();
                textSpans.AddRange(rightExpression.GetRealTextSpans());

                result.InitialTextSpans = textSpans;
                result.TextSpan = textSpans.Union();
                result.Root = binaryOperatorExpression.Root;
            }
            else
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
            Ust result = null;

            if (op.UnaryOperator == UnaryOperator.Minus)
            {
                string foldedValue = "";
                if (expression is IntLiteral intLiteral)
                {
                    long intValue = intLiteral.Value;
                    result = new IntLiteral(-intValue);
                    foldedValue = (-intValue).ToString();
                }
                else if (expression is FloatLiteral floatLiteral)
                {
                    double floatValue = floatLiteral.Value;
                    result = new FloatLiteral(-floatValue);
                    foldedValue = (-floatValue).ToString();
                }

                if (result != null)
                {
                    List<TextSpan> textSpans = expression.GetRealTextSpans();
                    textSpans.Add(op.TextSpan);

                    result.InitialTextSpans = textSpans;
                    result.TextSpan = textSpans.Union();
                    result.Root = unaryOperatorExpression.Root;

                    if (Logger.IsLogDebugs)
                    {
                        Logger.LogDebug($"Unary expression {unaryOperatorExpression} folded to {foldedValue} at {result.TextSpan}");
                    }
                }
            }
            
            if (result == null)
            {
                result = VisitChildren(unaryOperatorExpression);
            }

            return result;
        }

        protected override Ust VisitChildren(Ust ust)
        {
            try
            {
                return propertyEnumerator.VisitProperties(ust, Visit);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ConversionException(ust.Root?.SourceCodeFile, ex)
                {
                    TextSpan = ust.TextSpan
                });
                return null;
            }
        }
    }
}
