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

namespace PT.PM.UstPreprocessing
{
    public class UstPreprocessor : UstVisitor, IUstPreprocessor
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Ust Preprocess(Ust ast)
        {
            Ust result;
            fileNode = null;
            result = ast.Type == UstType.Common ? (Ust)new MostCommonUst() : (Ust)new MostDetailUst();
            result.FileName = ast.FileName;
            result.SourceLanguages = ast.SourceLanguages;
            result.Root = (FileNode)Visit(ast.Root);
            result.Comments = ast.Comments.Select(comment => (CommentLiteral)Visit(comment)).ToArray();
            return result;
        }

        public UstNode Preprocess(UstNode astNode)
        {
            return Visit(astNode);
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
                    new TextSpan(binaryOperatorExpression.TextSpan),
                    binaryOperatorExpression.FileNode);
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

        protected override UstNode VisitChildren(UstNode astNode)
        {
            try
            {
                return base.VisitChildren(astNode);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ConversionException(astNode.FileNode?.FileName?.Text ?? "", ex) { TextSpan = astNode.TextSpan });
                return null;
            }
        }
    }
}
