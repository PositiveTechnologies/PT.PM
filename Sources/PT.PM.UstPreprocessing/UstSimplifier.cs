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
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;
using PT.PM.Patterns;
using PT.PM.Dsl;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.UstPreprocessing
{
    public class UstSimplifier : UstVisitor<UstNode>, IUstPatternVisitor<UstNode>, IUstPreprocessor
    {
        protected FileNode fileNode;

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

        public override UstNode Visit(UstNode ustNode)
        {
            if (ustNode == null)
            {
                return null;
            }
            return Visit((dynamic)ustNode);
        }

        public override UstNode Visit(EntityDeclaration entityDeclaration)
        {
            if (entityDeclaration == null)
            {
                return null;
            }
            return Visit((dynamic)entityDeclaration);
        }

        public override UstNode Visit(Statement statement)
        {
            if (statement == null)
            {
                return null;
            }
            return Visit((dynamic)statement);
        }

        public override UstNode Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }
            return Visit((dynamic)expression);
        }

        public override UstNode Visit(Token literal)
        {
            if (literal == null)
            {
                return null;
            }
            return Visit((dynamic)literal);
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
            UnaryOperatorLiteral op = unaryOperatorExpression.Operator;
            Expression ex = unaryOperatorExpression.Expression;

            if (op.UnaryOperator == UnaryOperator.Minus)
            {
                if (ex.NodeType == NodeType.IntLiteral)
                {
                    long intValue = ((IntLiteral)ex).Value;
                    UstNode result = new IntLiteral
                    {
                        Value = -intValue,
                        FileNode = unaryOperatorExpression.FileNode,
                        TextSpan = op.TextSpan.Union(ex.TextSpan)
                    };
                    Logger.LogDebug($"Unary expression {unaryOperatorExpression} has been folded to {-intValue} at {result.TextSpan}");
                    return result;
                }

                if (ex.NodeType == NodeType.FloatLiteral)
                {
                    double doubleValue = ((FloatLiteral)ex).Value;
                    UstNode result = new FloatLiteral
                    {
                        Value = -doubleValue,
                        FileNode = unaryOperatorExpression.FileNode,
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

        public UstNode Visit(PatternNode patternVars)
        {
            UstNode data = Visit(patternVars.Node);
            List<PatternVarDef> vars = patternVars.Vars.Select(v => (PatternVarDef)Visit(v)).ToList();
            vars.Sort();
            return new PatternNode(data, vars);
        }

        public UstNode Visit(PatternExpressions patternExpressions)
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

        public UstNode Visit(PatternStatements patternStatements)
        {
            // ... ... ... -> ...
            List<Statement> collection = patternStatements.Statements
                .Select(item => (Statement)Visit(item)).ToList();
            int index = 0;
            while (index < collection.Count)
            {
                if (collection[index].NodeType == NodeType.PatternMultipleStatements &&
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

        public UstNode Visit(PatternVarDef patternVarDef)
        {
            List<Expression> vars = patternVarDef.Values.Select(v => (Expression)Visit(v)).ToList();
            vars.Sort();
            return new PatternVarDef(patternVarDef.Id, vars, patternVarDef.TextSpan);
        }

        public UstNode Visit(PatternBooleanLiteral patternBooleanLiteral)
        {
            return VisitChildren(patternBooleanLiteral);
        }

        public UstNode Visit(PatternComment patternComment)
        {
            return VisitChildren(patternComment);
        }

        public UstNode Visit(PatternExpression patternExpression)
        {
            return VisitChildren(patternExpression);
        }

        public UstNode Visit(PatternExpressionInsideExpression patternExpressionInsideExpression)
        {
            return VisitChildren(patternExpressionInsideExpression);
        }

        public UstNode Visit(PatternExpressionInsideStatement patternExpressionInsideStatement)
        {
            return VisitChildren(patternExpressionInsideStatement);
        }

        public UstNode Visit(PatternIdToken patternIdToken)
        {
            return VisitChildren(patternIdToken);
        }

        public UstNode Visit(PatternIntLiteral patternIntLiteral)
        {
            return VisitChildren(patternIntLiteral);
        }

        public UstNode Visit(PatternMultipleExpressions patternMultiExpressions)
        {
            return VisitChildren(patternMultiExpressions);
        }

        public UstNode Visit(PatternMultipleStatements patternMultiStatements)
        {
            return VisitChildren(patternMultiStatements);
        }

        public UstNode Visit(PatternStatement patternStatement)
        {
            return VisitChildren(patternStatement);
        }

        public UstNode Visit(PatternStringLiteral patternStringLiteral)
        {
            return VisitChildren(patternStringLiteral);
        }

        public UstNode Visit(PatternTryCatchStatement patternTryCatchStatement)
        {
            return VisitChildren(patternTryCatchStatement);
        }

        public UstNode Visit(PatternVarRef patternVarRef)
        {
            return VisitChildren(patternVarRef);
        }

        public UstNode Visit(DslNode patternExpression)
        {
            return VisitChildren(patternExpression);
        }

        public UstNode Visit(LangCodeNode langCodeNode)
        {
            return VisitChildren(langCodeNode);
        }

        protected override UstNode VisitChildren(UstNode ustNode)
        {
            try
            {
                if (ustNode == null)
                {
                    return null;
                }

                Type type = ustNode.GetType();
                PropertyInfo[] properties = ReflectionCache.GetClassProperties(type);

                var result = (UstNode)Activator.CreateInstance(type);
                foreach (PropertyInfo prop in properties)
                {
                    Type propType = prop.PropertyType;
                    if (propType.IsValueType || propType == typeof(string))
                    {
                        prop.SetValue(result, prop.GetValue(ustNode));
                    }
                    else if (typeof(UstNode).IsAssignableFrom(propType) && propType.Name != nameof(UstNode.Parent))
                    {
                        UstNode getValue = (UstNode)prop.GetValue(ustNode);
                        UstNode setValue = VisitNodeOrIgnoreFileNode(getValue);
                        prop.SetValue(result, setValue);
                        if (setValue != null)
                        {
                            setValue.Parent = result;
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
                            foreach (var item in sourceCollection)
                            {
                                var ustNodeItem = item as UstNode;
                                if (ustNodeItem != null)
                                {
                                    var destUstNodeItem = VisitNodeOrIgnoreFileNode(ustNodeItem);
                                    destCollection.Add(destUstNodeItem);
                                    if (destUstNodeItem != null)
                                    {
                                        destUstNodeItem.Parent = result;
                                    }
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
                        // ignore regex as they assignment via strings.
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
                Logger.LogError(new ConversionException(ustNode.FileNode?.FileName?.Text ?? "", ex) { TextSpan = ustNode.TextSpan });
                return null;
            }
        }

        protected UstNode VisitNodeOrIgnoreFileNode(UstNode node)
        {
            if (node == null)
            {
                return null;
            }
            else if (node.NodeType != NodeType.FileNode)
            {
                return Visit(node); // Prevent endless recursion.
            }
            else
            {
                if (fileNode == null)
                {
                    var fileNode = (FileNode)node;
                    this.fileNode = new FileNode(fileNode?.FileName?.Text, fileNode?.FileData);
                }
                return fileNode;
            }
        }
    }
}
