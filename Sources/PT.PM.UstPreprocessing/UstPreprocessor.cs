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
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.UstPreprocessing
{
    public class UstPreprocessor : UstVisitor, IUstPreprocessor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Ust Preprocess(Ust ust)
        {
            Ust result;
            fileNode = null;
            result = ust.Type == UstType.Common ? (Ust)new MostCommonUst() : (Ust)new MostDetailUst();
            result.FileName = ust.FileName;
            result.SourceLanguages = ust.SourceLanguages;
            result.Root = (FileNode)Visit(ust.Root);
            result.Comments = ust.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();
            return result;
        }

        public UstNode Preprocess(UstNode ustNode)
        {
            return Visit(ustNode);
        }

        public override UstNode Visit(BinaryOperatorExpression binaryOperatorExpression)
        {
            Expression result = null;
            Expression leftExpression = Visit((dynamic)binaryOperatorExpression.Left);
            BinaryOperatorLiteral op = Visit((dynamic)binaryOperatorExpression.Operator);
            Expression rightExpression = Visit((dynamic)binaryOperatorExpression.Right);

            if (leftExpression.NodeType == NodeType.StringLiteral &&
                rightExpression.NodeType == NodeType.StringLiteral)
            {
                string leftValue = ((StringLiteral)leftExpression).Text;
                string rightValue = ((StringLiteral)rightExpression).Text;
                if (op.BinaryOperator == BinaryOperator.Plus)
                {
                    string resultText = leftValue + rightValue;
                    result = new StringLiteral
                    {
                        Text = resultText,
                        FileNode = binaryOperatorExpression.FileNode,
                        TextSpan = leftExpression.TextSpan.Union(rightExpression.TextSpan)
                    };
                    Logger.LogDebug($"Strings {binaryOperatorExpression} has been concatenated to \"{resultText}\" at {result.TextSpan}");
                }
            }
            else if (leftExpression.NodeType == NodeType.IntLiteral &&
                rightExpression.NodeType == NodeType.IntLiteral)
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
                        FileNode = binaryOperatorExpression.FileNode,
                        TextSpan = leftExpression.TextSpan.Union(rightExpression.TextSpan)
                    };
                    Logger.LogDebug($"Arithmetic expression {binaryOperatorExpression} has been folded to {resultValue} at {result.TextSpan}");
                }
            }

            if (result == null)
            {
                result = new BinaryOperatorExpression(leftExpression, op, rightExpression,
                    new TextSpan(binaryOperatorExpression.TextSpan), binaryOperatorExpression.FileNode);
                leftExpression.Parent = result;
                rightExpression.Parent = result;
                op.Parent = result;
            }

            return result;
        }

        // Unify Statement to BlockStatement.
        public override UstNode Visit(IfElseStatement ifElseStatement)
        {
            Expression condition = (Expression)Visit(ifElseStatement.Condition);
            BlockStatement trueStatement = ConvertToBlockStatement((Statement)Visit(ifElseStatement.TrueStatement));
            BlockStatement falseStatement = ConvertToBlockStatement((Statement)Visit(ifElseStatement.FalseStatement));
            var result = new IfElseStatement(condition, trueStatement, ifElseStatement.TextSpan, ifElseStatement.FileNode);
            result.Condition.Parent = result;
            result.TrueStatement.Parent = result;
            if (result.FalseStatement != null)
            {
                result.FalseStatement.Parent = result;
            }
            return result;
        }

        public override UstNode Visit(UnaryOperatorExpression unaryOperatorExpression)
        {
            UstNode result;
            UnaryOperatorLiteral op = unaryOperatorExpression.Operator;
            Expression ex = unaryOperatorExpression.Expression;
            bool isOperatorMinus = op.UnaryOperator == UnaryOperator.Minus;
            bool isExpressionInt = ex.NodeType == NodeType.IntLiteral;
            bool isExpressionFloat = ex.NodeType == NodeType.FloatLiteral;
            if (isOperatorMinus && isExpressionInt)
            {
                long intValue = ((IntLiteral)ex).Value;
                result = new IntLiteral
                {
                    Value = -intValue,
                    FileNode = unaryOperatorExpression.FileNode,
                    TextSpan = op.TextSpan.Union(ex.TextSpan)
                };
                Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-intValue} at {result.TextSpan}");
            }
            else if(isOperatorMinus && isExpressionFloat)
            {
                double doubleValue = ((FloatLiteral)ex).Value;
                result = new FloatLiteral
                {
                    Value = -doubleValue,
                    FileNode = unaryOperatorExpression.FileNode,
                    TextSpan = op.TextSpan.Union(ex.TextSpan)
                };
                Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-doubleValue} at {result.TextSpan}");
            }
            else
            {
                result = VisitChildren(unaryOperatorExpression);
            }
            return result;
        }

        private BlockStatement ConvertToBlockStatement(Statement statement)
        {
            BlockStatement result;
            if (statement == null)
            {
                result = null;
            }
            else if (statement.NodeType == NodeType.BlockStatement)
            {
                result = (BlockStatement)statement;
            }
            else
            {
                result = new BlockStatement(new Statement[] { statement }, statement.TextSpan, statement.FileNode);
                statement.Parent = result;
            }
            return result;
        }

        public override UstNode Visit(PatternNode patternVars)
        {
            UstNode data = Visit(patternVars.Node);
            List<PatternVarDef> vars = patternVars.Vars.Select(v => (PatternVarDef)Visit(v)).ToList();
            vars.Sort();
            return new PatternNode(data, vars);
        }

        public override UstNode Visit(PatternExpressions patternExpressions)
        {
            // #* #* ... #* -> #*
            List<Expression> collection = patternExpressions.Collection
                .Select(item => (Expression)Visit(item)).ToList();
            int index = 0;
            while (index < collection.Count)
            {
                if (collection[index].NodeType == NodeType.PatternMultipleExpressions &&
                    index + 1 < collection.Count &&
                    collection[index + 1].NodeType == NodeType.PatternMultipleExpressions)
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

        public override UstNode Visit(PatternStatements patternStatements)
        {
            // ... ... ... -> ...
            List<Statement> collection = patternStatements.Statements
                .Select(item => (Statement)Visit(item)).ToList();
            int index = 0;
            while (index < collection.Count)
            {
                if (collection[index].NodeType ==  NodeType.PatternMultipleStatements &&
                    index + 1 < collection.Count &&
                    collection[index + 1].NodeType == NodeType.PatternMultipleStatements)
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

        public override UstNode Visit(PatternVarDef patternVarDef)
        {
            List<Expression> vars = patternVarDef.Values.Select(v => (Expression)Visit(v)).ToList();
            vars.Sort();
            return new PatternVarDef(patternVarDef.Id, vars, patternVarDef.TextSpan);
        }

        protected override UstNode VisitChildren(UstNode ustNode)
        {
            try
            {
                return base.VisitChildren(ustNode);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(ustNode.FileNode?.FileName?.Text ?? "", ex) { TextSpan = ustNode.TextSpan });
                return null;
            }
        }
    }
}
